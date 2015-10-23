namespace Overlay_informationLight
{
    internal class ShowMeMoreHelper
    {
        public string Modifier;
        public string EffectName;
        public string SecondeffectName;
        public int Range;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="effectName"></param>
        /// <param name="secondeffectName"></param>
        /// <param name="range"></param>
        public ShowMeMoreHelper(string modifier, string effectName, string secondeffectName, int range)
        {
            Modifier = modifier;
            this.EffectName = effectName;
            SecondeffectName = secondeffectName;
            Range = range;
        }
    }
}