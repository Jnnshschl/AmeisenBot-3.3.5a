﻿namespace AmeisenAI.Combat
{
    public abstract class CombatStructures
    {
        public enum CombatLogicStatement
        {
            GREATER,
            GREATER_OR_EQUAL,
            EQUAL,
            LESS_OR_EQUAL,
            LESS,
            HAS_BUFF,
            HAS_BUFF_MYSELF,
            NOT_HAS_BUFF,
            NOT_HAS_BUFF_MYSELF
        }

        public enum CombatLogicAction
        {
            USE_SPELL,
            USE_AOE_SPELL,
            SHAPESHIFT,
            FLEE
        }

        public enum CombatLogicValues
        {
            MYSELF_HP,
            MYSELF_ENERGY,
            //MYSELF_STUNNED,
            TARGET_HP,
            //TARGET_STUNNED,
            //TARGET_IS_CASTING,
        }
    }
}
