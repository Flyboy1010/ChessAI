using Godot;
using System;
using System.Collections.Generic;

namespace Chess
{
    public class Board
    {
        public readonly char[] boardLayout = {
			'R','N','B','Q','K','B','N','R',
			'P','P','P','P','P','P','P','P',
			' ',' ',' ',' ',' ',' ',' ',' ',
			' ',' ',' ',' ',' ',' ',' ',' ',
			' ',' ',' ',' ',' ',' ',' ',' ',
			' ',' ',' ',' ',' ',' ',' ',' ',
			'P','P','P','P','P','P','P','P',
			'R','N','B','Q','K','B','N','R',
		};

        public int[] pieces;

        private Stack<Move> previousMoves;

        public Move lastMove;

        // ctor
        public Board()
        {
            pieces = new int[64];

            previousMoves = new Stack<Move>();

            lastMove = null;
        }

        /* SETS UP THE BOARD ARRAY */

        public void CreateBoard()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int i = x + y * 8;
                    int color = (y < 4) ? Piece.black : Piece.white;

                    switch (boardLayout[i])
                    {
                        case 'P':
                            pieces[i] = Piece.pawn | color;
                            break;
                        case 'R':
                            pieces[i] = Piece.rook | color;
                            break;
                        case 'N':
                            pieces[i] = Piece.knight | color;
                            break;
                        case 'B':
                            pieces[i] = Piece.bishop | color;
                            break;
                        case 'Q':
                            pieces[i] = Piece.queen | color;
                            break;
                        case 'K':
                            pieces[i] = Piece.king | color;
                            break;
                        default:
                            pieces[i] = Piece.none;
                            break;
                    }
                }
            }
        }

        public void MakeMove(Move move, bool testing = false)
        {
            move.sourcePiece = pieces[move.sourceSquare];
            move.targetPiece = pieces[move.targetSquare];

            pieces[move.targetSquare] = move.sourcePiece | Piece.moved;
            pieces[move.sourceSquare] = Piece.none;

            /* FLAGS */

            if(move.canPromote)
            {
                pieces[move.targetSquare] = Piece.queen | Piece.GetPieceColor(move.sourcePiece) | Piece.moved;
            }
            else if(move.canCastle)
            {
                move.sourceRookPiece = pieces[move.rookSquareSource];

                pieces[move.rookSquareTarget] = move.sourceRookPiece;
                pieces[move.rookSquareSource] = Piece.none;
            }

            /* ADD THE MOVE TO THE STACK */

            previousMoves.Push(move);

            /* IF IS TESTING DO NOT SET THE LAST MOVE TO THIS MOVE */
            
            if(!testing) lastMove = move;
        }

        public void UnMakeMove(bool testing = false)
        {
            if (previousMoves.Count > 0)
            {
                Move move = previousMoves.Pop();

                pieces[move.sourceSquare] = move.sourcePiece;
                pieces[move.targetSquare] = move.targetPiece;

                /* FLAGS */

                if (move.canPromote)
                {
                    // do nothing
                }
                else if (move.canCastle)
                {
                    pieces[move.rookSquareSource] = move.sourceRookPiece;
                    pieces[move.rookSquareTarget] = Piece.none;
                }

                /* SET LAST MOVE */
                
                if(!testing) lastMove = move;
            }
        }
    }
}