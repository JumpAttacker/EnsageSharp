namespace OverlayInformation
{
    internal static class Checker
    {
        public static bool IsActive()
        {
            return Members.Menu.Item("Enable").GetValue<bool>();
        }
    }
}