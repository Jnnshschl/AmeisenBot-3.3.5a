﻿using AmeisenLogging;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenCore
{
    /// <summary>
    /// Class that manages the hooking of WoW's EndScene
    /// 
    /// !!! W.I.P !!!
    /// </summary>
    public class AmeisenHook
    {
        public bool isHooked = false;
<<<<<<< HEAD
        public bool isHookUsed = false;
=======
        public bool isInjectionUsed = false;
>>>>>>> 0569b2d7a73ab32a1711a3211e79422aaffb804a

        private uint codeCave;
        private uint codeCaveForInjection;
        private uint codeToExecute;
        private uint returnAdress;

        uint endsceneReturnAddress;

        private byte[] originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };

        public AmeisenHook() { Hook(); }

        public void Hook()
        {
            if (AmeisenManager.GetInstance().GetBlackMagic().IsProcessOpen)
            {
                // Get D3D9 Endscene Pointer
                uint endscene = GetEndScene();

                // If WoW is already hooked, unhook it
                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) == 0xE9)
                {
                    originalEndscene = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
                    DisposeHooking();
                }

                // If WoW is now/was unhooked, hook it
                if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) != 0xE9)
                {
                    uint endsceneHookOffset = 0x2;
                    endscene += endsceneHookOffset;

                    endsceneReturnAddress = endscene + 0x5;

                    //originalEndscene = AmeisenManager.GetInstance().GetBlackMagic().ReadBytes(endscene, 5);

                    codeToExecute = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(4);
                    AmeisenManager.GetInstance().GetBlackMagic().WriteInt(codeToExecute, 0);

                    returnAdress = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(4);
                    AmeisenManager.GetInstance().GetBlackMagic().WriteInt(returnAdress, 0);

                    codeCave = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(32);
                    codeCaveForInjection = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(64);

                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "EndScene at: " + endscene.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "EndScene returning at: " + (endsceneReturnAddress).ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeCave at:" + codeCave.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeCaveForInjection at:" + codeCaveForInjection.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "CodeToExecute at:" + codeToExecute.ToString("X"), this);
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Original Endscene bytes: " + Utils.ByteArrayToString(originalEndscene), this);

                    AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(codeCave, originalEndscene);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("PUSHFD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("PUSHAD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV EAX, [" + (codeToExecute) + "]");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("TEST EAX, 1");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JE @out");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV EAX, " + (codeCaveForInjection));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("CALL EAX");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV [" + (returnAdress) + "], EAX");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV EDX, 0");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("MOV [" + (codeToExecute) + "], EDX");

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("@out:");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPAD");
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("POPFD");
                    int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(codeCave + 5);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject((codeCave + (uint)asmLenght) + 5);

                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (codeCave));
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(endscene);

                }
                isHooked = true;
            }

        }

        public void DisposeHooking()
        {
            // Get D3D9 Endscene Pointer
            uint endscene = GetEndScene();

            uint endsceneHookOffset = 0x2;
            endscene += endsceneHookOffset;

            // Check if WoW is hooked
            if (AmeisenManager.GetInstance().GetBlackMagic().ReadByte(endscene) == 0xE9)
            {
                AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(endscene, originalEndscene);

                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeCave);
                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeToExecute);
                AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(codeCaveForInjection);
            }

            isHooked = false;
        }

        public byte[] InjectAndExecute(string[] asm)
        {
<<<<<<< HEAD
            while (isHookUsed)
            {
                Thread.Sleep(5);
            }

            isHookUsed = true;
=======
            while (isInjectionUsed)
                Thread.Sleep(5);

            isInjectionUsed = true;
>>>>>>> 0569b2d7a73ab32a1711a3211e79422aaffb804a

            AmeisenManager.GetInstance().GetBlackMagic().WriteInt(codeToExecute, 1);
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Clear();

            if (asm != null)
                foreach (string s in asm)
                    AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine(s);

            //AmeisenManager.GetInstance().GetBlackMagic().Asm.AddLine("JMP " + (endsceneReturnAddress));

            int asmLenght = AmeisenManager.GetInstance().GetBlackMagic().Asm.Assemble().Length;
            AmeisenManager.GetInstance().GetBlackMagic().Asm.Inject(codeCaveForInjection);

            while (AmeisenManager.GetInstance().GetBlackMagic().ReadInt(codeToExecute) > 0)
<<<<<<< HEAD
            {
                Thread.Sleep(5);
            }
=======
                Thread.Sleep(5);
>>>>>>> 0569b2d7a73ab32a1711a3211e79422aaffb804a

            byte buffer = new Byte();
            List<byte> returnBytes = new List<byte>();

            try
            {
                uint dwAddress = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(returnAdress);

                buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                while (buffer != 0)
                {
                    returnBytes.Add(buffer);
                    dwAddress = dwAddress + 1;
                    buffer = AmeisenManager.GetInstance().GetBlackMagic().ReadByte(dwAddress);
                }
            }
            catch (Exception e) { AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Crash at reading returnAddress: " + e.ToString(), this); }

<<<<<<< HEAD
            isHookUsed = false;

            return retnByte.ToArray();
=======
            isInjectionUsed = false;
            return returnBytes.ToArray();
>>>>>>> 0569b2d7a73ab32a1711a3211e79422aaffb804a
        }

        private uint GetEndScene()
        {
            uint pDevice = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(WoWOffsets.devicePtr1);
            uint pEnd = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pDevice + WoWOffsets.devicePtr2);
            uint pScene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pEnd);
            uint endscene = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(pScene + WoWOffsets.endScene);
            return endscene;
        }
    }
}
