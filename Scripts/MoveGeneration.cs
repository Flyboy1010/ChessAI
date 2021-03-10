using Godot;
using System;
using System.Collections.Generic;

namespace Chess
{
    public static class MoveGeneration
    {
        public static readonly int[] directions = { 8, -1, -8, 1, 7, -9, -7, 9 };
        public static readonly int[][] numSquaresToEdge;
        public static readonly int[][] preComputedKnightMoves;
        public static readonly int[][] preComputedKingMoves;

        static MoveGeneration()
        {
            /* 
                        PRECOMPUTE MOVE DATA 
                THIS IS IMPORTANT FOR PERFORMANCE REASONS,
                YOU CANT DO IT IN THE FLY FOR EACH PIECE,
                YOU MUST PRECALCULATE EVERY MOVE IF POSIBLE
            */

            numSquaresToEdge = new int[64][];
            preComputedKnightMoves = new int[64][];
            preComputedKingMoves = new int[64][];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int index = x + y * 8;

                    int up = 7 - y;
                    int right = x;
                    int down = y;
                    int left = 7 - x;

                    int d1 = Math.Min(right, up);
                    int d2 = Math.Min(right, down);
                    int d3 = Math.Min(left, down);
                    int d4 = Math.Min(left, up);

                    numSquaresToEdge[index] = new int[8] { up, right, down, left, d1, d2, d3, d4 };

                    /* KNIGHT MOVES */

                    List<int> knightMoves = new List<int>();

                    for (int j = -2; j <= 2; j += 4)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            if (x + i >= 0 && x + i < 8 && y + j >= 0 && y + j < 8)
                            {
                                knightMoves.Add((x + i) + (y + j) * 8);
                            }

                            if (x + j >= 0 && x + j < 8 && y + i >= 0 && y + i < 8)
                            {
                                knightMoves.Add((x + j) + (y + i) * 8);
                            }
                        }
                    }

                    preComputedKnightMoves[index] = knightMoves.ToArray();

                    /* KING MOVES */

                    List<int> kingMoves = new List<int>();

                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            if (x + i >= 0 && x + i < 8 && y + j >= 0 && y + j < 8)
                            {
                                kingMoves.Add((x + i) + (y + j) * 8);
                            }
                        }
                    }

                    preComputedKingMoves[index] = kingMoves.ToArray();
                }
            }
        }

        public static void GenerateSlidingMoves(Board board, int sourceSquare, List<Move> pseudoLegalMoves)
        {
            int piece = board.pieces[sourceSquare];

            int startDirectionIndex = Piece.IsPieceType(piece, Piece.rook) || Piece.IsPieceType(piece, Piece.queen) ? 0 : 4;
            int endDirectionIndex = Piece.IsPieceType(piece, Piece.rook) ? 4 : 8;

            for (int d = startDirectionIndex; d < endDirectionIndex; d++)
            {
                for (int i = 0; i < numSquaresToEdge[sourceSquare][d]; i++)
                {
                    int targetSquare = sourceSquare + (i + 1) * directions[d];

                    int targetPiece = board.pieces[targetSquare];

                    if(Piece.IsPieceType(targetPiece, Piece.none))
                    {
                        pseudoLegalMoves.Add(new Move(sourceSquare, targetSquare));
                    }
                    else if(Piece.IsPieceColor(targetPiece, Piece.GetPieceColor(piece)))
                    {
                        break;
                    }
                    else
                    {
                        pseudoLegalMoves.Add(new Move(sourceSquare, targetSquare));
                        break;
                    }
                }
            }
        }

        // TODO: this is messy, could be improved, also add en passant
        public static void GeneratePawnMoves(Board board, int sourceSquare, List<Move> pseudoLegalMoves)
        {
            int piece = board.pieces[sourceSquare];

            int up = Piece.IsPieceColor(piece, Piece.black) ? directions[0] : directions[2];

            if((Piece.IsPieceColor(piece, Piece.black) && numSquaresToEdge[sourceSquare][0] > 0) || (Piece.IsPieceColor(piece, Piece.white) && numSquaresToEdge[sourceSquare][2] > 0))
            {
                int targetSquareUp = sourceSquare + up;

                if(Piece.IsPieceType(board.pieces[targetSquareUp], Piece.none))
                {
                    Move move = new Move(sourceSquare, targetSquareUp);

                    if((Piece.IsPieceColor(piece, Piece.black) && numSquaresToEdge[sourceSquare][0] == 1) || (Piece.IsPieceColor(piece, Piece.white) && numSquaresToEdge[sourceSquare][2] == 1))
                    {
                        move.canPromote = true;
                    }
                    pseudoLegalMoves.Add(move);

                    if(!Piece.HasMoved(piece))
                    {
                        int targetSquareDoubleUp = targetSquareUp + up;

                        if (Piece.IsPieceType(board.pieces[targetSquareDoubleUp], Piece.none))
                        {
                            pseudoLegalMoves.Add(new Move(sourceSquare, targetSquareDoubleUp));
                        }
                    }
                }

                if(numSquaresToEdge[sourceSquare][1] > 0)
                {
                    int targetSquareUpRight = targetSquareUp + directions[1];

                    if(!Piece.IsPieceType(board.pieces[targetSquareUpRight], Piece.none) && Piece.GetPieceColor(piece) != Piece.GetPieceColor(board.pieces[targetSquareUpRight]))
                    {
                        Move move = new Move(sourceSquare, targetSquareUpRight);

                        if((Piece.IsPieceColor(piece, Piece.black) && numSquaresToEdge[sourceSquare][0] == 1) || (Piece.IsPieceColor(piece, Piece.white) && numSquaresToEdge[sourceSquare][2] == 1))
                        {
                            move.canPromote = true;
                        }

                        pseudoLegalMoves.Add(move);
                    }
                }

                if(numSquaresToEdge[sourceSquare][3] > 0)
                {
                    int targetSquareUpLeft = targetSquareUp + directions[3];

                    if(!Piece.IsPieceType(board.pieces[targetSquareUpLeft], Piece.none) && Piece.GetPieceColor(piece) != Piece.GetPieceColor(board.pieces[targetSquareUpLeft]))
                    {
                        Move move = new Move(sourceSquare, targetSquareUpLeft);

                        if((Piece.IsPieceColor(piece, Piece.black) && numSquaresToEdge[sourceSquare][0] == 1) || (Piece.IsPieceColor(piece, Piece.white) && numSquaresToEdge[sourceSquare][2] == 1))
                        {
                            move.canPromote = true;
                        }

                        pseudoLegalMoves.Add(move);

                    }
                }
            }
        }

        public static void GenerateKnightMoves(Board board, int sourceSquare, List<Move> pseudoLegalMoves)
        {
            int piece = board.pieces[sourceSquare];

            foreach(var square in preComputedKnightMoves[sourceSquare])
            {
                if(Piece.IsPieceType(board.pieces[square], Piece.none))
                {
                    pseudoLegalMoves.Add(new Move(sourceSquare, square));
                }
                else if(Piece.GetPieceColor(piece) != Piece.GetPieceColor(board.pieces[square]))
                {
                    pseudoLegalMoves.Add(new Move(sourceSquare, square));
                }
            }
        }

        public static void GenerateKingMoves(Board board, int sourceSquare, List<Move> pseudoLegalMoves)
        {
            int piece = board.pieces[sourceSquare];

            foreach(var square in preComputedKingMoves[sourceSquare])
            {
                if(Piece.IsPieceType(board.pieces[square], Piece.none))
                {
                    pseudoLegalMoves.Add(new Move(sourceSquare, square));
                }
                else if(Piece.GetPieceColor(piece) != Piece.GetPieceColor(board.pieces[square]))
                {
                    pseudoLegalMoves.Add(new Move(sourceSquare, square));
                }
            }

            /* CASTLELING */

            if(!Piece.HasMoved(piece))
            {
                /* LONG CASTLE */

                if(Piece.IsPieceType(board.pieces[sourceSquare + directions[1]], Piece.none) && Piece.IsPieceType(board.pieces[sourceSquare + 2 * directions[1]], Piece.none) && Piece.IsPieceType(board.pieces[sourceSquare + 3 * directions[1]], Piece.none))
                {
                    int rookPiece = board.pieces[sourceSquare + 4 * directions[1]];

                    if(Piece.IsPieceType(rookPiece, Piece.rook))
                    {
                        if(!Piece.HasMoved(rookPiece))
                        {
                            Move move = new Move(sourceSquare, sourceSquare + 2 * directions[1]);

                            move.canCastle = true;

                            move.rookSquareSource = sourceSquare + 4 * directions[1];

                            move.rookSquareTarget = sourceSquare + directions[1];

                            pseudoLegalMoves.Add(move);
                        }
                    }
                }

                /* SHORT CASTLE */

                if(Piece.IsPieceType(board.pieces[sourceSquare + directions[3]], Piece.none) && Piece.IsPieceType(board.pieces[sourceSquare + 2 * directions[3]], Piece.none))
                {
                    int rookPiece = board.pieces[sourceSquare + 3 * directions[3]];

                    if(Piece.IsPieceType(rookPiece, Piece.rook))
                    {
                        if(!Piece.HasMoved(rookPiece))
                        {
                            Move move = new Move(sourceSquare, sourceSquare + 2 * directions[3]);

                            move.canCastle = true;

                            move.rookSquareSource = sourceSquare + 3 * directions[3];

                            move.rookSquareTarget = sourceSquare + directions[3];

                            pseudoLegalMoves.Add(move);
                        }
                    }
                }
            }
        }

        public static List<Move> GeneratePieceMoves(Board board, int sourceSquare)
        {
            List<Move> pseudoLegalMoves = new List<Move>();

            int piece = board.pieces[sourceSquare];

            switch(Piece.GetPieceType(piece))
            {
                case Piece.pawn:
                    GeneratePawnMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
                case Piece.knight:
                    GenerateKnightMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
                case Piece.bishop:
                    GenerateSlidingMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
                case Piece.queen:
                    GenerateSlidingMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
                case Piece.rook:
                    GenerateSlidingMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
                case Piece.king:
                    GenerateKingMoves(board, sourceSquare, pseudoLegalMoves);
                    break;
            }

            return pseudoLegalMoves;
        }

        public static List<Move> GenerateAllMoves(Board board, int color)
        {
            List<Move> pseudoLegalMoves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                int piece = board.pieces[i];

                if(!Piece.IsPieceType(piece, Piece.none))
                {
                    if(Piece.IsPieceColor(piece, color))
                    {
                        switch(Piece.GetPieceType(piece))
                        {
                            case Piece.pawn:
                                GeneratePawnMoves(board, i, pseudoLegalMoves);
                                break;
                            case Piece.knight:
                                GenerateKnightMoves(board, i, pseudoLegalMoves);
                                break;
                            case Piece.bishop:
                                GenerateSlidingMoves(board, i, pseudoLegalMoves);
                                break;
                            case Piece.queen:
                                GenerateSlidingMoves(board, i, pseudoLegalMoves);
                                break;
                            case Piece.rook:
                                GenerateSlidingMoves(board, i, pseudoLegalMoves);
                                break;
                            case Piece.king:
                                GenerateKingMoves(board, i, pseudoLegalMoves);
                                break;
                        }
                    }
                }
            }

            return pseudoLegalMoves;
        }


        // TODO: optimize this
        public static bool IsInCheck(Board board, int color)
        {
            List<Move> enemyMoves = GenerateAllMoves(board, color == Piece.white ? Piece.black : Piece.white);

            foreach(var move in enemyMoves)
            {
                int piece = board.pieces[move.targetSquare];

                if(Piece.IsPieceType(piece, Piece.king))
                {
                    if(Piece.GetPieceColor(piece) == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<Move> GenerateLegalMoves(Board board, int sourceSquare)
        {
            List<Move> pseudoLegalMoves = GeneratePieceMoves(board, sourceSquare);
            List<Move> legalMoves = new List<Move>();

            int color = Piece.GetPieceColor(board.pieces[sourceSquare]);

            foreach(var move in pseudoLegalMoves)
            {
                board.MakeMove(move, true);

                if(!IsInCheck(board, color))
                {
                    legalMoves.Add(move);
                }

                board.UnMakeMove(true);
            }

            return legalMoves;
        }
    }
}
