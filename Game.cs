using System;
using System.Runtime.InteropServices;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using static OOBlugin.OOBlugin;

namespace OOBlugin
{
    public static unsafe class Game
    {
        public delegate void ProcessChatBoxDelegate(nint uiModule, nint message, nint unused, byte a4);
        public static ProcessChatBoxDelegate ProcessChatBox;
        public static nint uiModule = nint.Zero;

        private static nint walkingBoolPtr = nint.Zero;
        public static bool IsWalking
        {
            get => walkingBoolPtr != nint.Zero && *(bool*)walkingBoolPtr;
            set
            {
                if (walkingBoolPtr != nint.Zero)
                {
                    *(bool*)walkingBoolPtr = value;
                    *(bool*)(walkingBoolPtr - 0x10B) = value; // Autorun
                }
            }
        }

        private delegate nint GetModuleDelegate(nint basePtr);

        public static nint newGameUIPtr = nint.Zero;
        public delegate nint GetUnknownNGPPtrDelegate();
        public static GetUnknownNGPPtrDelegate GetUnknownNGPPtr;
        public delegate void NewGamePlusActionDelegate(nint a1, nint a2);
        public static NewGamePlusActionDelegate NewGamePlusAction;

        public static nint emoteAgent = nint.Zero;
        public delegate void DoEmoteDelegate(nint agent, uint emoteID, long a3, bool a4, bool a5);
        public static DoEmoteDelegate DoEmote;

        public static nint contentsFinderMenuAgent = nint.Zero;
        public delegate void OpenAbandonDutyDelegate(nint agent);
        public static OpenAbandonDutyDelegate OpenAbandonDuty;

        public static nint itemContextMenuAgent = nint.Zero;
        public delegate void UseItemDelegate(nint itemContextMenuAgent, uint itemID, uint inventoryPage, uint inventorySlot, short a5);
        public static UseItemDelegate UseItem;

        private static int* keyStates;
        private static byte* keyStateIndexArray;
        public static byte GetKeyStateIndex(int key) => key is >= 0 and < 240 ? keyStateIndexArray[key] : (byte)0;
        private static ref int GetKeyState(int key) => ref keyStates[key];

        public static bool SendKey(System.Windows.Forms.Keys key) => SendKey((int)key);
        public static bool SendKey(int key)
        {
            var stateIndex = GetKeyStateIndex(key);
            if (stateIndex <= 0) return false;

            GetKeyState(stateIndex) |= 6;
            return true;
        }
        public static bool SendKeyHold(System.Windows.Forms.Keys key) => SendKeyHold((int)key);
        public static bool SendKeyHold(int key)
        {
            var stateIndex = GetKeyStateIndex(key);
            if (stateIndex <= 0) return false;

            GetKeyState(stateIndex) |= 1;
            return true;
        }
        public static bool SendKeyRelease(System.Windows.Forms.Keys key) => SendKeyRelease((int)key);
        public static bool SendKeyRelease(int key)
        {
            var stateIndex = GetKeyStateIndex(key);
            if (stateIndex <= 0) return false;

            GetKeyState(stateIndex) &= ~1;
            return true;
        }

        public static void Initialize()
        {
            /*try
            {
                uiModule = DalamudApi.GameGui.GetUIModule();
                ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(DalamudApi.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));
            }
            catch { PrintError("Failed to load /qexec"); }

            try { walkingBoolPtr = DalamudApi.SigScanner.GetStaticAddressFromSig("40 38 35 ?? ?? ?? ?? 75 2D"); } // also found around g_PlayerMoveController+523
            catch { PrintError("Failed to load /walk"); }
            */
            try
            {
                var agentModule = Framework.Instance()->GetUIModule()->GetAgentModule();

                try
                {
                    GetUnknownNGPPtr = Marshal.GetDelegateForFunctionPointer<GetUnknownNGPPtrDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? 66 39 78 08"));
                    NewGamePlusAction = Marshal.GetDelegateForFunctionPointer<NewGamePlusActionDelegate>(DalamudApi.SigScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B 02 48 8B F2"));
                    newGameUIPtr = (nint)agentModule->GetAgentByInternalId(AgentId.QuestRedo) + 0xD0;
                }
                catch { PrintError("Failed to load /ng+t"); }

                try
                {
                    DoEmote = Marshal.GetDelegateForFunctionPointer<DoEmoteDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B8 0A 00 00 00"));
                    emoteAgent = (nint)agentModule->GetAgentByInternalId(AgentId.Emote);
                }
                catch { PrintError("Failed to load /doemote"); }

                try
                {
                    OpenAbandonDuty = Marshal.GetDelegateForFunctionPointer<OpenAbandonDutyDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 90 48 8B 4B 10"));
                    contentsFinderMenuAgent = (nint)agentModule->GetAgentByInternalId(AgentId.ContentsFinderMenu);
                }
                catch { PrintError("Failed to load /leaveduty"); }

                /*try
                {
                    UseItem = Marshal.GetDelegateForFunctionPointer<UseItemDelegate>(DalamudApi.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38"));
                    itemContextMenuAgent = (nint)agentModule->GetAgentByInternalId(AgentId.InventoryContext);
                }
                catch { PrintError("Failed to load /useitem"); }*/
            }
            catch { PrintError("Failed to get agent module"); }

            try
            {
                keyStates = (int*)DalamudApi.SigScanner.GetStaticAddressFromSig("4C 8D 05 ?? ?? ?? ?? 44 8B 0D"); // 4C 8D 05 ?? ?? ?? ?? 44 8B 1D
                keyStateIndexArray = (byte*)(DalamudApi.SigScanner.Module.BaseAddress + *(int*)(DalamudApi.SigScanner.ScanModule("0F B6 94 33 ?? ?? ?? ?? 84 D2") + 4));
            }
            catch { PrintError("Failed to load /sendkey!"); }
        }

        public static void Dispose()
        {

        }
    }
}
