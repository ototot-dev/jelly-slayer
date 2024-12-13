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
        Worker,
        Alien,
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
        GuardParrying,
        SuperArmor,
        MoveSpeedUp,
        ActionSpeedUp,
        HPRegen,            // 체력 회복

        __DEBUFF__SEPERATOR__,
        Groggy,             //* 그로기
        Staggered,          //* 경직
        KnockDown,          //* 다운
        Guardbreak,         //* 가드 뚫림
        CanNotRoll, 
        CanNotJump,
        CanNotMove,
        CanNotGuard,
        CanNotAction,
        MoveSpeedDown,
        ActionSpeedDown,
        ActionStaminaLow,
        Bind,               // 끈에 묶임
    }


    [UGS(typeof(ActionResults))]
    public enum ActionResults
    {
        None = 0,
        Damaged,
        Missed,
        Blocked,
        GuardBreak,
        GuardParried,
        KickParried,
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