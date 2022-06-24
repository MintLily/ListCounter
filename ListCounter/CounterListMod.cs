using MelonLoader;
using System.Collections;
using System;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

[assembly: MelonInfo(typeof(ListCounter.Main), 
    ListCounter.BuildInfo.Name, 
    ListCounter.BuildInfo.Version, 
    ListCounter.BuildInfo.Author, 
    ListCounter.BuildInfo.DownloadLink)]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ListCounter;

internal static class BuildInfo {
    public const string Name = "ListCounter";
    public const string Author = "Lily";
    public const string Version = "1.0.2";
    public const string Company = "Minty Labs";
    public const string DownloadLink = "https://github.com/MintLily/ListCounter";
}

public class Main : MelonMod {
    private static int _totalFriends, _scenesLoaded;
    private UiUserList? _onlineFriendsList;
    // private UiAvatarList _avatarList;
    private Text? _onlineFriendsText, /*_avatarsText,*/ _inRoomText;
    private static bool _hasLoadedOnUi, _hasOpenedSocialMenu/*, HasOpenedAvatarMenu*/, _stillSomehowThrowingAnError;
    
    #region Mod Logic
    
    private static IEnumerator UpdateMembersText(Text? textObj, UiUserList? online, int total) {
        yield return new WaitForSeconds(1);
        if (_stillSomehowThrowingAnError) yield break;
        if (!_showOnlineFriends!.Value && !_showTotalFriends!.Value) yield break;
        
        textObj!.text = $"Online Friends ({(_showOnlineFriends.Value ? $"{online!.field_Private_Int32_0}/" : "")}{(_showTotalFriends!.Value ? $"{total}" : "")})";
    }
    
    private static IEnumerator UpdateInRoomText(Text? textObj) {
        yield return new WaitForSeconds(1);
        if (_stillSomehowThrowingAnError) yield break;
        if (!_showInRoomCount!.Value) yield break;
        
        textObj!.text = $"In Room ({PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count})";
    }
    
    /*private static IEnumerator UpdateAvatarsText(Text textObj, UiAvatarList total) {
        yield return new WaitForSeconds(1);
        if (!_showAvatarCount!.Value) yield break;
        
        textObj.text = $"My Creations ({total.field_Private_Dictionary_2_String_ApiAvatar_0.Count})";
    }*/
    
    private void OnOpenSocialMenu() {
        MelonCoroutines.Start(UpdateMembersText(_onlineFriendsText, _onlineFriendsList, _totalFriends));
        MelonCoroutines.Start(UpdateInRoomText(_inRoomText));
        _hasOpenedSocialMenu = true;
    }
        
    private void OnCloseSocialMenu() => _hasOpenedSocialMenu = false;
    
    /*private void OnOpenAvatarMenu() {
        MelonCoroutines.Start(UpdateAvatarsText(_avatarsText, _avatarList));
        _hasOpenedAvatarMenu = true;
    }
    
    private void OnCloseAvatarMenu() => _hasOpenedAvatarMenu = false;*/

    private void RegisterListeners() {
        var socialMenu = GameObject.Find("UserInterface/MenuContent/Screens/Social");
        // avatarMenu = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
        
        var socialMenuListener = socialMenu.GetOrAddComponent<EnableDisableListener>();
        socialMenuListener.OnEnabled += OnOpenSocialMenu;
        socialMenuListener.OnDisabled += OnCloseSocialMenu;
        
        Debug("Finished Creating Social Menu Listener.");
        
        /*var avatarMenuListener = avatarMenu.GetOrAddComponent<EnableDisableListener>();
        avatarMenuListener.OnEnabled += OnOpenAvatarMenu;
        avatarMenuListener.OnDisabled += OnCloseAvatarMenu;
        
        Debug("Finished Creating Avatar Menu Listener.");*/
    }
    
    #endregion

    #region MelonMod Logic

    private bool _isDebug;
    private readonly MelonLogger.Instance _logger = new (BuildInfo.Name, ConsoleColor.Green);
    private static MelonPreferences_Category? _counterList;
    private static MelonPreferences_Entry<bool>? _showTotalFriends, _showOnlineFriends, /*_showAvatarCount,*/ _showInRoomCount;

    public override void OnApplicationStart() {
        if (MelonDebug.IsEnabled() || Environment.CommandLine.Contains("--cl.debug")) {
            _isDebug = true;
            Log("Debug mode is active");
        }
        
        _counterList = MelonPreferences.CreateCategory(BuildInfo.Name, BuildInfo.Name);
        _showTotalFriends = _counterList.CreateEntry("ShowTotalFriends", true, "Show Total Friends");
        _showOnlineFriends = _counterList.CreateEntry("ShowOnlineFriends", true, "Show Online Friends");
        _showInRoomCount = _counterList.CreateEntry("ShowInRoomCount", true, "Show In Room Count");
        //_showAvatarCount = _counterList.CreateEntry("ShowAvatarCount", true, "Show Avatar Count");
        
        bool failed;
        try { ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>(); failed = false; }
        catch (Exception e) { _logger.Error($"Unable to Inject Custom EnableDisableListener Script!\n{e}"); failed = true; }
        if (!failed) Debug("Finished setting up EnableDisableListener");
    }
    
    private IEnumerator DoTheUi(float timer) {
        yield return new WaitForSeconds(timer);
        var onlineFriendsViewport = GameObject.Find("UserInterface/MenuContent/Screens/Social/Vertical Scroll View/Viewport/Content/OnlineFriends");
        var friendsListTextObj = GameObject.Find("UserInterface/MenuContent/Screens/Social/Vertical Scroll View/Viewport/Content/OnlineFriends/Button/TitleText");
        _totalFriends = APIUser.CurrentUser.friendIDs._size;
        _onlineFriendsList = onlineFriendsViewport.GetComponent<UiUserList>();
        _onlineFriendsText = friendsListTextObj.GetComponent<Text>();
        Debug("Got Friends List");
        
        var inRoomListTextObj = GameObject.Find("UserInterface/MenuContent/Screens/Social/Vertical Scroll View/Viewport/Content/InRoom/Button/TitleText");
        _inRoomText = inRoomListTextObj.GetComponent<Text>();
        Debug("Got In Room List");
        
        /*var avatarsList = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Personal Avatar List");
        var avatarCreationTextObj = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Personal Avatar List/Button/TitleText");
        _avatarList = avatarsList.GetComponent<UiAvatarList>();
        _avatarsText = avatarCreationTextObj.GetComponent<Text>();
        Debug("Got Avatar List");*/
        _logger.Warning("Personal Creation Avatar Count was removed.");
        
        _hasLoadedOnUi = true;
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
        if (_scenesLoaded > 2) return;
        _scenesLoaded++;
        if (_scenesLoaded != 2) return;
        
        RegisterListeners();

        try {
            MelonCoroutines.Start(DoTheUi(2f)); // try it
        }
        catch (Exception ex) {
            try {
                Log("Oops, I failed, let's try that again in 5 seconds.");
                MelonCoroutines.Start(DoTheUi(5f)); // failed? let's wait a little bit and try again
            }
            catch (Exception ex2) {
                _stillSomehowThrowingAnError = true; // still failing, ok, lets stop and not continue throwing errors
                _logger.Error($"Exception #1:\n{ex}");
                _logger.Error($"Exception #2:\n{ex2}");
            }
        }
    }

    #endregion
    
    private void Log(string s) => _logger.Msg(s);

    private void Debug(string s) {
        if (_isDebug) _logger.Msg(ConsoleColor.DarkMagenta, s);
    }
}