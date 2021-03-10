using Godot;
using System;
using System.Collections.Generic;

namespace Chess
{
	[Tool]
    public class BoardUI : Node2D
    {
        [Export]
		public PieceTheme pieceTheme;

        [Export]
        public Texture squareTexture;

        [Export]
        public int scale = 80;

        public Sprite[] piecesSprites = new Sprite[64];
		private Sprite[] squaresSprites = new Sprite[64];

        private Sprite boardSprite;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            boardSprite = GetNode<Sprite>("BoardSprite");

            boardSprite.Texture = pieceTheme.boardSprite;
            boardSprite.Scale = new Vector2(scale / 150f, scale / 150f);
        }

        public void CreateBoardUI(Board board)
		{
			for(int y = 0; y < 8; y++)
			{
				for(int x = 0; x < 8; x++)
				{
                    /* GET THE INDEX AND PIECE OF THE BOARD */

                    int i = x + y * 8;
                    int piece = board.pieces[i];

                    /* CREATE SQUARE SPRITES */

                    squaresSprites[i] = new Sprite();
					squaresSprites[i].Position = new Vector2(x * scale, y * scale);
					squaresSprites[i].Texture = squareTexture;
					squaresSprites[i].SelfModulate = new Color(0, 0, 0, 0);
                    squaresSprites[i].Centered = false;
                    squaresSprites[i].Scale = new Vector2(scale / 150f, scale / 150f);

                    AddChild(squaresSprites[i]);

					/* CREATE PIECE SPRITES */

					piecesSprites[i] = new Sprite();
					piecesSprites[i].Position = new Vector2(x * scale, y * scale);
					piecesSprites[i].Texture = pieceTheme.GetTexture(board.pieces[i]);
                    piecesSprites[i].Centered = false;
					piecesSprites[i].Scale = new Vector2(scale / 150f, scale / 150f);

					AddChild(piecesSprites[i]);
                }   
			}
		}

		public void UpdateBoardPieces(Board board)
		{
			for(int y = 0; y < 8; y++)
			{
				for(int x = 0; x < 8; x++)
				{
                    /* PIECES SPRITES POSITIONS */

                    int piece = board.pieces[x + y * 8];

					piecesSprites[x + y * 8].Position = new Vector2(x * scale, y * scale);
					piecesSprites[x + y * 8].ZIndex = 0;
					piecesSprites[x + y * 8].Texture = pieceTheme.GetTexture(piece);
				}
			}
		}

		public void ResetBoardSquares()
		{
			for(int i = 0; i < 64; i++)
			{
				squaresSprites[i].SelfModulate = new Color(0, 0, 0, 0);
			}
		}

		public void ViewLegalMovements(Board board, List<Move> legalMovements)
		{
			foreach(var move in legalMovements)
			{
                squaresSprites[move.targetSquare].SelfModulate = Piece.IsPieceType(board.pieces[move.targetSquare], Piece.none) ? pieceTheme.colorValidMove : pieceTheme.colorPieceWarning;
            }
		}

		public void ViewLastMove(Board board)
		{
            Move lastMove = board.lastMove;

            if(lastMove != null)
			{
                squaresSprites[lastMove.sourceSquare].SelfModulate = pieceTheme.colorLastMove;
                squaresSprites[lastMove.targetSquare].SelfModulate = pieceTheme.colorLastMove;
            }
		}

    }
}
