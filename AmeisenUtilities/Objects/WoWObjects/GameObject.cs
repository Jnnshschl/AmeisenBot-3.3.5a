﻿using AmeisenUtilities;
using Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenUtilities
{
    public class GameObject : WoWObject
    {
        public GameObject(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();

            /*pos.x = BlackMagicInstance.ReadFloat(BaseAddress + 0x3C);
            pos.y = BlackMagicInstance.ReadFloat(BaseAddress + 0x40);
            pos.z = BlackMagicInstance.ReadFloat(BaseAddress + 0x44);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x28);*/

            // too cpu heavy
            /*try
            {
                distance = Utils.GetDistance(pos, AmeisenManager.GetInstance().Me().pos);
            }
            catch { }*/
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("GAMEOBJECT");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> Name: " + Name);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            return sb.ToString();
        }
    }
}