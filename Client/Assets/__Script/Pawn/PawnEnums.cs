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
        RoboDog,
        Worker,
        Alien,
        Etasphera42,
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
        SuperArmor,         // 슈퍼 아머 (삭제 예정)
        MoveSpeedUp,        // 이동 속도 증가
        ActionSpeedUp,      // 액션 속도 증가
        HPRegen,            // 체력 회복
        IncreasePoise,      // 강인도 증가
        __DEBUFF__SEPERATOR__,
        Groggy,             //* 그로기
        Staggered,          //* 경직
        KnockDown,          //* 다운
        KnockBack,          //* 넉백
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
        Burst,              // 분노 폭발 상태
    }

    [UGS(typeof(HitTypes))]
    public enum HitTypes
    {
        None = 0,
        Hit,
        Kick,
        Slash,
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
        PunchParried,
        ProjectileReflected,
    }

    public enum RootMotionConstraints : int
    {
        None = 0,
        FreezePositionX = 0x01,
        FreezePositionY = 0x02,
        FreezePositionZ = 0x04,
        FreezeRotationPitch = 0x10,
        FreezeRotationYaw = 0x20,
        FreezeRotationRoll = 0x40,
    }

    public enum SuperArmorLevels : int
    {
        None = 0,
        CanNotStraggerOnBlacked,    //* 블럭킹에 의해서 Action이 캔슬되지 않음
        CanNotStarggerOnDamaged,    //* 데미지에 의해서 Action이 캔슬되지 않음
        Max,
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