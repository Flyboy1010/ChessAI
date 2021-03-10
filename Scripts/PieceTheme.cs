using Godot;
using System;

namespace Chess
{
    [Tool]
    public class PieceTheme : Resource
    {
        [Export]
        public Texture boardSprite;

        [Export]
		public Color colorLastMove;

		[Export]
		public Color colorValidMove;

		[Export]
		public Color colorPieceWarning;

        [Export]
        public Texture[] pawnSprite = new Texture[2];

        [Export]
        public Texture[] rookSprite = new Texture[2];

        [Export]
        public Texture[] bishopSprite = new Texture[2];

        [Export]
        public Texture[] knightSprite = new Texture[2];

        [Export]
        public Texture[] queenSprite = new Texture[2];

        [Export]
        public Texture[] kingSprite = new Texture[2];

        public Texture GetTexture(int piece)
        {
            switch(Piece.GetPieceType(piece))
            {
                case Piece.pawn:
                    return pawnSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                case Piece.rook:
                    return rookSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                case Piece.bishop:
                    return bishopSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                case Piece.knight:
                    return knightSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                case Piece.queen:
                    return queenSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                case Piece.king:
                    return kingSprite[Piece.GetPieceColor(piece) == Piece.white ? 0 : 1];
                default:
                    return null;
            }
        }
    }
}
