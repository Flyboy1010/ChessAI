using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Chess
{
	[Tool]
	public class GameManager : Node
	{
		[Export]
		public int depth = 4;

		private Board board;

		private BoardUI boardUI;

		private AI aI;

		private AudioStreamPlayer moveSound;
		private AudioStreamPlayer captureSound;

		private int selectedPieceSquare;
		private bool isPieceSelected;

		private List<Move> legalMoves;

		private bool turnPlayer = true;
		private bool calculating = false;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			boardUI = GetNode<BoardUI>("BoardUI");
			moveSound = GetNode<AudioStreamPlayer>("MoveSound");
			captureSound = GetNode<AudioStreamPlayer>("CaptureSound");

			board = new Board();

			aI = new AI(depth);

			legalMoves = new List<Move>();

			board.CreateBoard();
			boardUI.CreateBoardUI(board);
		}

		private bool IsInBounds(int x, int y)
		{
			return (x >= 0 && x < 8 && y >= 0 && y < 8);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			if (!Engine.EditorHint)
			{
				if (turnPlayer)
				{
					if (Input.IsActionJustPressed("Select"))
					{
						// get mouse position
						Vector2 mouse = GetViewport().GetMousePosition() - boardUI.Position;

						// get position in grid
						int squareX = (int)(mouse.x / boardUI.scale);
						int squareY = (int)(mouse.y / boardUI.scale);
						int selectedSquare = squareX + squareY * 8;

						// check if it is inside the bounds
						if (IsInBounds(squareX, squareY))
						{
							// get the piece if there is one
							if (!Piece.IsPieceType(board.pieces[selectedSquare], Piece.none) && Piece.IsPieceColor(board.pieces[selectedSquare], Piece.white))
							{
								selectedPieceSquare = selectedSquare;
								isPieceSelected = true;

								legalMoves = MoveGeneration.GenerateLegalMoves(board, selectedSquare);
							}
                            else
                            {
                                legalMoves.Clear();
                            }
                        }
                        else
                        {
                            legalMoves.Clear();
                        }
                    }
					else if (Input.IsActionJustReleased("Select"))
					{
						if (isPieceSelected)
						{
							// get mouse position
							Vector2 mouse = GetViewport().GetMousePosition() - boardUI.Position;

							int squareX = (int)(mouse.x / boardUI.scale);
							int squareY = (int)(mouse.y / boardUI.scale);
							int targetSquare = squareX + squareY * 8;

							// check if it is inside the bounds
							if (IsInBounds(squareX, squareY))
							{
								foreach (var move in legalMoves)
								{
									if (targetSquare == move.targetSquare)
									{
										if (Piece.IsPieceType(board.pieces[targetSquare], Piece.none)) moveSound.Play();
										else captureSound.Play();

										board.MakeMove(move);

										turnPlayer = false;

                                        legalMoves.Clear();

                                        break;
									}
								}
							}

                            boardUI.UpdateBoardPieces(board);

							isPieceSelected = false;
						}
					}

					if (Input.IsActionPressed("Select"))
					{
						if (isPieceSelected)
						{
							// get mouse position
							Vector2 mouse = GetViewport().GetMousePosition() - boardUI.Position;

							Sprite piece = boardUI.piecesSprites[selectedPieceSquare];
							piece.Position = mouse + new Vector2(-boardUI.scale / 2f, -boardUI.scale / 2f);
							piece.ZIndex = 1;
						}
					}

					if (Input.IsActionJustPressed("Restore"))
					{
						// unmake both moves your move and black pieces move
						board.UnMakeMove();
						board.UnMakeMove();

                        boardUI.UpdateBoardPieces(board);

						legalMoves.Clear();
                    }
				}
				else
				{
					/* 
						I DONT THINK THIS IS THE RIGHT WAY TO DO THIS
						IF I DONT USE A THREAD, IF IT TAKES "TOO MUCH TIME"
						WINDOWS WILL THINK IT STOPPED WORKING,
						I AM NOT REALLY HAPPY WITH THIS SOLUTION
					*/
					if(!calculating)
					{
						calculating = true;

						System.Threading.Thread t = new System.Threading.Thread(new ThreadStart(ComputerTurn));

						t.Start();
					}
				}

                boardUI.ResetBoardSquares();
                boardUI.ViewLastMove(board);
                boardUI.ViewLegalMovements(board, legalMoves);
            }
		}

		public void ComputerTurn()
		{
			Move bestMove = aI.GetBestMove(board);

            if (bestMove != null)
			{
				if(Piece.IsPieceType(board.pieces[bestMove.targetSquare], Piece.none)) moveSound.Play();
				else captureSound.Play();

				board.MakeMove(bestMove);

				boardUI.UpdateBoardPieces(board);

				moveSound.Play();

				turnPlayer = true;

				calculating = false;
			}
			else
			{
				GD.Print("Checkmate");
			}
		}
	}
}
