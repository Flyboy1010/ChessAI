using Godot;
using System;

namespace Chess
{
    public static class Piece
    {
        // PIECE VALUES

        public const int none = 0;
        public const int pawn = 1;
        public const int rook = 2;
        public const int bishop = 3;
        public const int knight = 4;
        public const int queen = 5;
        public const int king = 6;

        // MASKS
        
        public const int black = 0b001000;
        public const int white = 0b010000;

        public const int moved = 0b100000;

        public const int pieceMask = 0b000111;
        public const int blackMask = 0b001000;
        public const int whiteMask = 0b010000;
        public const int colorMask = blackMask | whiteMask;
        public const int movedMask = 0b100000;

        public static int GetPieceType(int piece)
        {
            return piece & pieceMask;
        }

        public static int GetPieceColor(int piece)
        {
            return piece & colorMask;
        }

        public static bool IsPieceType(int piece, int pieceType)
        {
            return (piece & pieceMask) == pieceType;
        }

        public static bool IsSlidingPiece(int piece)
        {
            return (IsPieceType(piece, Piece.rook) || IsPieceType(piece, Piece.bishop) || IsPieceType(piece, Piece.queen));
        }

        public static bool IsPieceColor(int piece, int color)
        {
            return (piece & colorMask) == color;
        }

        public static bool HasMoved(int piece)
        {
            return (piece & movedMask) == moved;
        }
    }
}
