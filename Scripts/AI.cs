using Godot;
using System;
using System.Collections.Generic;

namespace Chess
{
    public class AI
    {
        private int depth;

        private const int pawnValue = 100;
        private const int rookValue = 479;
        private const int bishopValue = 320;
        private const int knightValue = 280;
        private const int queenValue = 929;
        private const int kingValue = 60000;

        private readonly int[] bestPawnPositions = {
             100, 100, 100, 100, 105, 100, 100,  100,
              78,  83,  86,  73, 102,  82,  85,  90,
               7,  29,  21,  44,  40,  31,  44,   7,
             -17,  16,  -2,  15,  14,   0,  15, -13,
             -26,   3,  10,   9,   6,   1,   0, -23,
             -22,   9,   5, -11, -10,  -2,   3, -19,
             -31,   8,  -7, -37, -36, -14,   3, -31,
               0,   0,   0,   0,   0,   0,   0,   0
        };

        private readonly int[] bestKnightPositions = {
            -66, -53, -75, -75, -10, -55, -58, -70,
             -3,  -6, 100, -36,   4,  62,  -4, -14,
             10,  67,   1,  74,  73,  27,  62,  -2,
             24,  24,  45,  37,  33,  41,  25,  17,
             -1,   5,  31,  21,  22,  35,   2,   0,
            -18,  10,  13,  22,  18,  15,  11, -14,
            -23, -15,   2,   0,   2,   0, -23, -20,
            -74, -23, -26, -24, -19, -35, -22, -69
        };

        private readonly int[] bestBishopPositions = {
            -59, -78, -82, -76, -23,-107, -37, -50,
            -11,  20,  35, -42, -39,  31,   2, -22,
             -9,  39, -32,  41,  52, -10,  28, -14,
             25,  17,  20,  34,  26,  25,  15,  10,
             13,  10,  17,  23,  17,  16,   0,   7,
             14,  25,  24,  15,   8,  25,  20,  15,
             19,  20,  11,   6,   7,   6,  20,  16,
             -7,   2, -15, -12, -14, -15, -10, -10
        };

        private readonly int[] bestRookPositions = {
             35,  29,  33,   4,  37,  33,  56,  50,
             55,  29,  56,  67,  55,  62,  34,  60,
             19,  35,  28,  33,  45,  27,  25,  15,
              0,   5,  16,  13,  18,  -4,  -9,  -6,
            -28, -35, -16, -21, -13, -29, -46, -30,
            -42, -28, -42, -25, -25, -35, -26, -46,
            -53, -38, -31, -26, -29, -43, -44, -53,
            -30, -24, -18,   5,  -2, -18, -31, -32
        };

        private readonly int[] bestQueenPositions = {
              6,   1,  -8,-104,  69,  24,  88,  26,
             14,  32,  60, -10,  20,  76,  57,  24,
             -2,  43,  32,  60,  72,  63,  43,   2,
              1, -16,  22,  17,  25,  20, -13,  -6,
            -14, -15,  -2,  -5,  -1, -10, -20, -22,
            -30,  -6, -13, -11, -16, -11, -16, -27,
            -36, -18,   0, -19, -15, -15, -21, -38,
            -39, -30, -31, -13, -31, -36, -34, -42
        };

        private readonly int[] bestKingPositions = {
              4,  54,  47, -99, -99,  60,  83, -62,
            -32,  10,  55,  56,  56,  55,  10,   3,
            -62,  12, -57,  44, -67,  28,  37, -31,
            -55,  50,  11,  -4, -19,  13,   0, -49,
            -55, -43, -52, -28, -51, -47,  -8, -50,
            -47, -42, -43, -79, -64, -32, -29, -32,
             -4,   3, -14, -50, -57, -18,  13,   4,
             17,  30,  -3, -14,   6,  -1,  40,  18
        };

        public AI(int _depth)
        {
            depth = _depth;
        }

        private int GetPieceValue(int piece)
        {
            switch(Piece.GetPieceType(piece))
            {
                case Piece.pawn:
                    return pawnValue;
                case Piece.rook:
                    return rookValue;
                case Piece.bishop:
                    return bishopValue;
                case Piece.knight:
                    return knightValue;
                case Piece.queen:
                    return queenValue;
                case Piece.king:
                    return kingValue;
            }

            return 0;
        }

        private void SortMoves(Board board, List<Move> moveList)
        {
            int[] moveScore = new int[moveList.Count];

            for (int i = 0; i < moveList.Count; i++)
            {
                moveScore[i] = 0;

                if(!Piece.IsPieceType(board.pieces[moveList[i].targetSquare], Piece.none))
                {
                    moveScore[i] += 10 * GetPieceValue(board.pieces[moveList[i].targetSquare]) - GetPieceValue(board.pieces[moveList[i].sourceSquare]);
                }

                if(moveList[i].canPromote)
                {
                    moveScore[i] += queenValue;
                }
            }

            for (int sorted = 0; sorted < moveList.Count; sorted++)
            {
                int bestScore = int.MinValue;
                int bestScoreIndex = 0;

                for (int i = sorted; i < moveList.Count; i++)
                {
                    if(moveScore[i] > bestScore)
                    {
                        bestScore = moveScore[i];
                        bestScoreIndex = i;
                    }
                }

                /* SWAP */

                Move bestMove = moveList[bestScoreIndex];
                moveList[bestScoreIndex] = moveList[sorted];
                moveList[sorted] = bestMove;
            }
        }

        private int Evaluate(Board board)
        {
            int totalValueBlack = 0;
            int totalValueWhite = 0;

            for(int i = 0; i < 64; i++)
            {
                int piece = board.pieces[i];

                if(!Piece.IsPieceType(piece, Piece.none))
                {
                    int pieceColor = Piece.GetPieceColor(piece);
                    int pieceValue = 0;
                    int index = (pieceColor == Piece.black) ? 63 - i : i;

                    switch(Piece.GetPieceType(board.pieces[i]))
                    {
                        case Piece.pawn:
                            pieceValue = pawnValue + bestPawnPositions[index];
                            break;
                        case Piece.knight:
                            pieceValue = knightValue + bestKnightPositions[index];
                            break;
                        case Piece.bishop:
                            pieceValue = bishopValue + bestBishopPositions[index];
                            break;
                        case Piece.rook:
                            pieceValue = rookValue + bestRookPositions[index];
                            break;
                        case Piece.queen:
                            pieceValue = queenValue + bestQueenPositions[index];
                            break;
                        case Piece.king:
                            pieceValue = kingValue + bestKingPositions[index];
                            break;
                    }

                    if(pieceColor == Piece.white) totalValueWhite += pieceValue;
                    else totalValueBlack += pieceValue;
                }
            }

            return totalValueBlack - totalValueWhite;
        }

        private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer)
        {
            if (depth == 0)
                return Evaluate(board);

            if(isMaximizingPlayer)
            {
                int bestValue = int.MinValue;

                List<Move> pseudoLegalMoves = MoveGeneration.GenerateAllMoves(board, Piece.black);

                SortMoves(board, pseudoLegalMoves);

                foreach(var move in pseudoLegalMoves)
                {
                    board.MakeMove(move, true);

                    if(MoveGeneration.IsInCheck(board, Piece.black))
                    {
                        board.UnMakeMove(true);
                        continue;
                    }

                    int value = Minimax(board, depth - 1, alpha, beta, false);

                    board.UnMakeMove(true);

                    bestValue = Math.Max(value, bestValue);

                    alpha = Math.Max(alpha, value);

                    if(beta <= alpha)
                    {
                        break;
                    }
                }

                return bestValue;
            }
            else
            {
                int bestValue = int.MaxValue;

                List<Move> pseudoLegalMoves = MoveGeneration.GenerateAllMoves(board, Piece.white);

                SortMoves(board, pseudoLegalMoves);

                foreach(var move in pseudoLegalMoves)
                {
                    board.MakeMove(move, true);

                    if(MoveGeneration.IsInCheck(board, Piece.white))
                    {
                        board.UnMakeMove(true);
                        continue;
                    }

                    int value = Minimax(board, depth - 1, alpha, beta, true);

                    board.UnMakeMove(true);

                    bestValue = Math.Min(value, bestValue);

                    beta = Math.Min(beta, value);

                    if(beta <= alpha)
                    {
                        break;
                    }
                }

                return bestValue;
            }
        }

        public Move GetBestMove(Board board)
        {
            int bestValue = int.MinValue;
            Move bestMove = null;

            List<Move> pseudoLegalMoves = MoveGeneration.GenerateAllMoves(board, Piece.black);

            SortMoves(board, pseudoLegalMoves);

            foreach(var move in pseudoLegalMoves)
            {
                board.MakeMove(move, true);

                if(MoveGeneration.IsInCheck(board, Piece.black))
                {
                    board.UnMakeMove(true);
                    continue;
                }

                int value = Minimax(board, depth, int.MinValue, int.MaxValue, false);

                board.UnMakeMove(true);

                if(value >= bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }
            }

            return bestMove;
        }
    }
}