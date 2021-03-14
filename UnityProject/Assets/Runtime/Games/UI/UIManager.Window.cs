using System.Collections.Generic;

public partial class UIManager
{
    struct WindowInfo
    {
        public UIWindowType windowType;
        public UIWindowLayer windowLayer;
    }

    private Dictionary<string, WindowInfo> _windowsInfo = new Dictionary<string, WindowInfo>(64);

    private void _AllWindowMeta()
    {
        // basic
        _WindowMeta(UINameConst.UIDecorationMain, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIMainGroup, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIInDeveloping, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIWaiting, UIWindowLayer.Waiting);
        _WindowMeta(UINameConst.UIMsgBox, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIMsgBoxWithTexture, UIWindowLayer.Tips);

        _WindowMeta(UINameConst.UICurrencyGroup, UIWindowLayer.Currency);
        _WindowMeta(UINameConst.UIStory, UIWindowLayer.Guide);
        _WindowMeta(UINameConst.UIGuidePortrait, UIWindowLayer.Guide);
        _WindowMeta(UINameConst.UIFinishedRoomSaveShare, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIRestaurantRankReward, UIWindowLayer.Notice);
        _WindowMeta(UINameConst.UIGallerySpin, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIUnlockPanel, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIDecorationExpBar, UIWindowLayer.Tips);

        _WindowMeta(UINameConst.UIContactUs, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIRateUs, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIRestaurantRankRewardPopup, UIWindowLayer.Notice);
        _WindowMeta(UINameConst.UIDeliveryPanel, UIWindowLayer.Notice);

        _WindowMeta(UINameConst.UITipBox, UIWindowLayer.Effect);
        _WindowMeta(UINameConst.WinStreakActivityDescription, UIWindowLayer.ForeGround);
        _WindowMeta(UINameConst.MysteryBoxNotice, UIWindowLayer.ForeGround);
        _WindowMeta(UINameConst.MysteryBoxHelp, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.MysteryBoxActivityDescription, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.MysteryBoxGetBuffReward, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.MysteryBoxEnabledWarning, UIWindowLayer.Notice);
        _WindowMeta(UINameConst.MysteryBoxDisabledWarning, UIWindowLayer.Notice);

        _WindowMeta(UINameConst.PiggyBankMainUI, UIWindowLayer.Normal);

        _WindowMeta(UINameConst.UITransition, UIWindowLayer.Effect);
        _WindowMeta(UINameConst.UISaveProgress, UIWindowLayer.Tips);


        // cooking
        _WindowMeta(UINameConst.UICookingGame, UIWindowLayer.BackGround);
        _WindowMeta(UINameConst.UILevelProperty, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICookerLevelUpAtRestaurant, UIWindowLayer.Tips);

        _WindowMeta(UINameConst.UIDailyTask, UIWindowLayer.ForeGround);
        _WindowMeta(UINameConst.UINotice3, UIWindowLayer.Notice);

        _WindowMeta(UINameConst.UIPublicReward, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.GiftGroup, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICookLevelComplete, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIStarBar, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIPopUpBox1, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIPopUpBox2, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICookingPause, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICookingMoreGuest, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICookingLevelFailed, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIFragmentCollectBubble, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UILevelRetry, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UINotice, UIWindowLayer.Notice);
        _WindowMeta(UINameConst.UIStarterPack, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIFirstRechargeNotice, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UILogin, UIWindowLayer.Normal);
        
//        _WindowMeta(UINameConst.GameShopPanel, UIWindowLayer.Normal);
//        _WindowMeta(UINameConst.Unlock, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UINoConnection, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UIDailyBonus, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.TimeLimitPassEnabledWarning, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.TimeLimitPassMainPanel, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.TipReward, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UIShopTimeLimitedActivityBundleNotice, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UIShopBuyGetFreeNotice, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UISecondDiscountNotice, UIWindowLayer.Tips);
//        _WindowMeta(UINameConst.UITVReward, UIWindowLayer.Tips);
        //worldmap
        _WindowMeta(UINameConst.WorldMap, UIWindowLayer.Normal);
        //old cooking
        _WindowMeta(UINameConst.HourlyBonus, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.Notice4, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.FBLoginFirst, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.AchievementsUI, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.Settings, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.FBMailBox, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.Mail, UIWindowLayer.Notice);
        _WindowMeta(UINameConst.InviteBackground, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIFBLike, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIBuyEnergy, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.ShopPurchase, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.ShopPurchase2, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.ShopPurchase3, UIWindowLayer.Tips);
        
        _WindowMeta(UINameConst.UIGiftRV, UIWindowLayer.Tips);

        //周末狂欢
        _WindowMeta(UINameConst.UICrazyTruckMain, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICrazyTruckMap, UIWindowLayer.BackGround);
        _WindowMeta(UINameConst.UICrazyTruckLevelProperty, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UICrazyTruckDebug, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckGiftTipBox, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckLevelCompleted, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckEnd, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckMapUI, UIWindowLayer.ForeGround);
        _WindowMeta(UINameConst.UICrazyTruckHelp, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckLevelFailed, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckNotice, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UICrazyTruckGet, UIWindowLayer.Tips);

        
        _WindowMeta(UINameConst.UIActivityRoom, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIActivityRoomProgress, UIWindowLayer.Tips);
        _WindowMeta(UINameConst.UIActivityRoomButton, UIWindowLayer.Normal);
        _WindowMeta(UINameConst.UIActivityRoomCountDown, UIWindowLayer.Notice);


        // to be deleted           
        _WindowMeta(UINameConst.UICameraDebug, UIWindowLayer.Effect);
    }


    private void _WindowMeta(string name, UIWindowLayer windowLayer)
    {
        _windowsInfo.Add(name, new WindowInfo { windowType = UIWindowType.PopupTip, windowLayer = windowLayer });
    }
}

public static class UINameConst
{
    public static readonly string UIFinishedRoomSaveShare = "UIFinishedRoomSaveShare";
    public static readonly string UIDecorationMain = "Home/UIDecorationMain";
    public static readonly string UIMainGroup = "Home/UIMainGroup";
    public static readonly string UIInDeveloping = "UIInDeveloping";
    public static readonly string UIMsgBox = "Home/UIMsgBox";
    public static readonly string UIMsgBoxWithTexture = "Home/UIMsgBoxWithTexture";
    public static readonly string UICurrencyGroup = "Home/UICurrencyGroup";
    public static readonly string UIStory = "Home/UIStory";
    public static readonly string UIGuidePortrait = "Home/UIGuidePortrait";
    public static readonly string UIPublicReward = "Home/UIPublicReward";
    public static readonly string UIRestaurantRankReward = "Home/UIRestaurantRankReward";


    public static readonly string UICameraDebug = "UICameraDebug";
    //public static readonly string UIPublicTextFly = "UIPublicTextFly";
//    public static readonly string GameShopPanel = GameShopPanelController.AssetPath;
//    public static readonly string UIDailyBonus= UIDailyBonusController.AssetPath;
//    public static readonly string Unlock = UnlockController.AssetPath;
//    public static readonly string UINoConnection = UINoConnectionController.AssetPath;
//    public static readonly string TimeLimitPassMainPanel = TimeLimitPassMainPanelController.AssetPath;
//    public static readonly string TimeLimitPassEnabledWarning = TimeLimitPassEnabledWarningController.AssetPath;
//    public static readonly string TipReward = TipRewardController.AssetPath;
//    public static readonly string UIShopTimeLimitedActivityBundleNotice = UIShopTimeLimitedActivityBundleNoticeController.AssetPath;
//    public static readonly string UIShopBuyGetFreeNotice = UIShopBuyGetFreeNoticeController.AssetPath;
//    public static readonly string UISecondDiscountNotice = UISecondDiscountNoticeController.AssetPath;
//    public static readonly string UITVReward = UITVRewardController.AssetPath;
    public static readonly string UIGallerySpin = "Home/UIGallerySpin";

    public static readonly string UIDecorationExpBar = "Home/UIDecorationExpBar";


    public static readonly string UIUnlockPanel = "Home/UIUnlockPanel";
    public static readonly string UITipBox = "Home/UITipBox";
    public static readonly string UIRestaurantRankRewardPopup = "Home/UIRestaurantRankRewardPopup";
    
    public static readonly string UISaveProgress = "Common/SaveProgress";
    public static readonly string UIExit = "Common/Exit";
    public static readonly string UINotice = "Common/Notice";
    public static readonly string UILogin = "Common/Login";



    //cooking
    public static readonly string UILevelProperty = "Common/LevelProperty";
    public static readonly string UICookerLevelUpAtRestaurant = "Common/CookerLevelUpAtRestaurantUI";

    public static readonly string UIDailyTask = "Common/UIDailyTask";
    public static readonly string UINotice3 = "Common/UINotice3";
    public static readonly string Notice4 = "Common/Notice4";
    public static readonly string HourlyBonus = "Common/HourlyBonus";
    public static readonly string UICookLevelComplete = "Common/UILevelCompleted";
    public static readonly string UIStarBar = "Common/UIStarBar";
    public static readonly string UICookingGame = "Common/GameUI";
    public static readonly string UICookingPause = "Common/Pause";
    public static readonly string UICookingMoreGuest = "Common/Moreguest";
    public static readonly string UICookingLevelFailed = "Common/LevelFailedUI";
    public static readonly string UIFBLike = "Common/FBLike";
    public static readonly string UIBuyEnergy = "Common/BuyEnergy";
    public static readonly string ShopPurchase = "Common/shopPurchase";
    public static readonly string ShopPurchase2 = "Common/shopPurchase2";
    public static readonly string ShopPurchase3 = "Common/shopPurchase3";
    public static readonly string UIFragmentCollectBubble = "Home/UIFragmentCollectBubble";
    public static readonly string UILevelRetry = "Common/LevelRetry";
    public static readonly string UIDeliveryPanel = "Home/UIDeliveryPanel";
    public static readonly string UIWaiting = "Common/Waiting";
    public static readonly string UIFirstRechargeNotice = "Home/UIFirstRechargeNotice";
    
    public static readonly string UIPopUpBox1 = "Common/UIPopUpBox01";
    public static readonly string UIPopUpBox2 = "Common/UIPopUpBox02";



    public static readonly string WinStreakActivityDescription = "WinStreakUltraCooker/WinStreakActivityDescription";
    public static readonly string MysteryBoxNotice = "MysteryBox/MysteryBoxNotice";
    public static readonly string MysteryBoxHelp = "MysteryBox/MysteryBoxHelp";
    public static readonly string MysteryBoxActivityDescription = "MysteryBox/MysteryBoxActivityDescription";
    public static readonly string MysteryBoxGetBuffReward = "MysteryBox/MysteryBoxGetBuffReward";
    public static readonly string MysteryBoxEnabledWarning = "MysteryBox/MysteryBoxEnabledWarning";
    public static readonly string MysteryBoxDisabledWarning = "MysteryBox/MysteryBoxDisabledWarning";

    public static readonly string PiggyBankMainUI = "PiggyBank/PiggyBankMainUI";

    public static readonly string UITransition = "Home/UITransition";
    public static readonly string GiftGroup = "Home/GiftGroup";


    // setting
    public static readonly string UIContactUs = "Common/UIContactUs";
    public static readonly string UIRateUs = "Common/UIRateUs";
    //UImaingroup
    public static readonly string Mail = "Common/Mail";
    public static readonly string FBMailBox = "Common/FBMailBox";
    public static readonly string Settings = "Common/Set";
    public static readonly string AchievementsUI = "Common/AchievementsUI";
    public static readonly string FBLoginFirst = "Common/FBLoginFirst";
    public static readonly string InviteBackground = "Common/InviteBackground";
    public static readonly string InviteItem = "Common/InviteItem";
    public static readonly string WorldMap = "WorldCommon/WorldMap";
    
    
    
    public static readonly string UIShopBaseBundleItem = "Home/UIShopBaseBundleItem";
    public static readonly string UIShopBaseCommodityItem = "Home/UIShopBaseCommodityItem";
    public static readonly string UIShopBaseCommodityAbbreviationItem = "Home/UIShopBaseCommodityAbbreviationItem";
    public static readonly string UIShopBaseBundleCommodityItem1 = "Home/UIShopBaseBundleCommodityItem1";
    public static readonly string UIShopTimeLimitedActivityBundleItem1 = "Home/UIShopTimeLimitedActivityBundleItem1";
    public static readonly string UIShopTimeLimitedActivityBundleItem2 = "Home/UIShopTimeLimitedActivityBundleItem2";

    public static readonly string UIStarterPack = "Common/StarterPack";

    public static readonly string UIGiftRV = "Common/RewardVideoMysteryGift";
    
    // 周末狂欢
    public static readonly string UICrazyTruckMain = "CrazyTruck/UI/CrazyTruckGameUI";
    public static readonly string UICrazyTruckMap = "CrazyTruck/UI/CrazyTruckWorldMap";
    public static readonly string UICrazyTruckMapUI = "CrazyTruck/UI/CrazyTruckWorldMapUI";
    public static readonly string UICrazyTruckLevelProperty = "CrazyTruck/UI/CrazyTruckLevelProperty";
    public static readonly string UICrazyTruckGiftTipBox = "CrazyTruck/UI/CrazyTruckGiftTipBox";
    public static readonly string UICrazyTruckLevelCompleted = "CrazyTruck/UI/CrazyTruckLevelCompleted";
    public static readonly string UICrazyTruckEnd = "CrazyTruck/UI/CrazyTruckEnd";
    public static readonly string UICrazyTruckHelp = "CrazyTruck/UI/CrazyTruckHelp";
    public static readonly string UICrazyTruckLevelFailed = "CrazyTruck/UI/CrazyTruckLevelFailedUI";
    public static readonly string UICrazyTruckNotice = "CrazyTruck/UI/CrazyTruckNotice";
    public static readonly string UICrazyTruckGet = "CrazyTruck/UI/CrazyTruckGet";

    public static readonly string UIActivityRoom = "ActivityRoomXmas/UIActivityRoomXmas";
    public static readonly string UIActivityRoomProgress = "Home/UIActivityRoomXmasExpBar";
    public static readonly string UIActivityRoomCountDown = "Common/CountDown";
    public static readonly string UIActivityRoomButton = "Home/ActivityRoomXmasButton";

    public static readonly string UICrazyTruckDebug = "CrazyTruck/UI/CrazyTruckDebug";
}