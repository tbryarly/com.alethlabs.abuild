namespace AlethEditor.Build
{
    public static class BuildMarketplacesExtends
    {
        public static string GetBuildDefine(this BuildMarketplaces marketplace)
        {
            switch (marketplace)
            {
                case BuildMarketplaces.Steam:
                    return "STEAMWORKS_NET";
                case BuildMarketplaces.Epic:
                    return "MARKETPLACE_EPIC";
                case BuildMarketplaces.Itch:
                    return "MARKETPLACE_ITCH";
                default:
                    return "";
            }
        }
    }
}