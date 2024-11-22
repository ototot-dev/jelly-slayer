using GoogleSheet.Core.Type;

namespace Game
{
    [UGS(typeof(PawnId))]
    public enum PawnId
    {
        None,
        Hero,
        DroneBot,
        Soldier,
        Zombie,
        Footman,
        Warrior,
        SlimeBoss,
        SlimeMini,
    }

    [UGS(typeof(PawnStatus))]
    public enum PawnStatus
    {
        None = 0,
        Invincible,         //* 일반 무적
        InvincibleDodge,    //* 구르기 무적
        PassiveParrying,
        SuperArmor,
        MoveSpeedUp,
        ActionSpeedUp,
        __DEBUFF__SEPERATOR__,
        Groggy,         //* 그로기
        Staggered,      //* 경직
        KnockDown,      //* 다운
        Guardbreak,     //* 가드 뚫림
        CanNotDash, 
        CanNotJump,
        CanNotMove,
        CanNotAction,
        MoveSpeedDown,
        ActionSpeedDown,
        ActionStaminaLow,
        Bind,           // 끈에 묶임
    }


    [UGS(typeof(ActionResults))]
    public enum ActionResults
    {
        None = 0,
        Damaged,
        Blocked,
        GuardBreak,
        ActiveParried,
        PassiveParried,
    }



    [UGS(typeof(ActionId))]
    public enum ActionId
    {
        None,
    }
    
    [UGS(typeof(BuffId))]
    public enum BuffId
    {
        None,
    }

    [UGS(typeof(BuffTypes))]
    public enum BuffTypes
    {
        None,
    }
}