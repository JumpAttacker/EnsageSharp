using SharpDX;

namespace Overlay_informationLight
{
    internal class ScreenSizer
    {
        private readonly int _floatRange;
        private readonly int _space;
        private readonly int _height;
        private readonly int _botRange;
        private readonly int _rangeBetween;
        private readonly Vector2 _menuPos;

        /// <summary>
        /// Init ur screen size
        /// </summary>
        /// <param name="floatRange">range between icons</param>
        /// <param name="space">range between teams</param>
        /// <param name="height"></param>
        /// <param name="botRange">start posY</param>
        /// <param name="rangeBetween">start posX</param>
        /// <param name="menuPos">jOverlay vector2 pos</param>
        public ScreenSizer(int floatRange, int space, int height, int botRange, int rangeBetween, Vector2 menuPos)
        {
            _floatRange = floatRange;
            _space = space;
            _height = height;
            _botRange = botRange;
            _rangeBetween = rangeBetween;
            _menuPos = menuPos;
        }

        public Vector2 MenuPos
        {
            get { return _menuPos; }
        }

        public int RangeBetween
        {
            get { return _rangeBetween; }
        }

        public int BotRange
        {
            get { return _botRange; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int Space
        {
            get { return _space; }
        }

        public int FloatRange
        {
            get { return _floatRange; }
        }
    }
}