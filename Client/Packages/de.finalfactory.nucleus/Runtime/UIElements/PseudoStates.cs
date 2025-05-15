using System;

namespace FinalFactory.UIElements
{
    [Flags]
    public enum PseudoStates
    {
        Active = 1,
        Hover = 2,
        Checked = 8,
        Disabled = 32, // 0x00000020
        Focus = 64, // 0x00000040
        Root = 128, // 0x00000080
    }
}