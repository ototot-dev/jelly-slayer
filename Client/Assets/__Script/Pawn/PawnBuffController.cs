using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnBuffController : MonoBehaviour
    {
        public Action<BuffTypes> onBuffActive;
        public Action<BuffTypes> onBuffDeactive;

#if UNITY_EDITOR
        public Dictionary<BuffTypes, List<Tuple<float, float>>> StackableBuffTable => __stackableBuffTable;
        public Dictionary<BuffTypes, Tuple<float, float>> UniqueBuffTable => __uniqueBuffTable;
        public HashSet<IBuffContainer> externBuffContainer = new();
#endif

        //* 중첩 가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<BuffTypes, List<Tuple<float, float>>> __stackableBuffTable = new();

        //* 중첩 불가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<BuffTypes, Tuple<float, float>> __uniqueBuffTable = new();

        //* 외부 버프, 중첩 불가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<IBuffContainer, Dictionary<BuffTypes, Tuple<float, float>>> __externBuffTables = new();

        //* 할당 불가능
        readonly HashSet<BuffTypes> __immunedBuffs = new();

        public bool RegisterBuffContainer(IBuffContainer container)
        {
#if UNITY_EDITOR
            externBuffContainer.Add(container);
#endif

            if (!__externBuffTables.ContainsKey(container))
            {
                __externBuffTables.Add(container, container.GetBuffTable());
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnregisterBuffContainer(IBuffContainer container)
        {
#if UNITY_EDITOR
            externBuffContainer.Remove(container);
#endif

            __externBuffTables.Remove(container);
        }

        public void SetImmunedBuff(params BuffTypes[] buffs)
        {
            foreach (var b in buffs)
                __immunedBuffs.Add(b);
        }
        
        public bool CheckBuff(BuffTypes buff)
        {
            return __uniqueBuffTable.ContainsKey(buff) || (__stackableBuffTable.ContainsKey(buff) && __stackableBuffTable[buff].Count > 0) || __externBuffTables.Any(e => e.Value.ContainsKey(buff));
        }

        public float GetBuffStrength(BuffTypes buff)
        {
            var ret = 0f;
            if (__uniqueBuffTable.ContainsKey(buff))
                ret =  __uniqueBuffTable[buff].Item1;
            if (__stackableBuffTable.ContainsKey(buff) && __stackableBuffTable[buff].Count > 0)
                ret = Mathf.Max(ret, __stackableBuffTable[buff].First().Item1);
            if (__externBuffTables.Count > 0 && __externBuffTables.Any(e => e.Value.ContainsKey(buff)))
                ret = Mathf.Max(ret, __externBuffTables.Max(e => e.Value[buff].Item1));

            return ret;
        }
        
        /// <summary>
        /// 특정 시간 동안 Buff를 넣어주는 함수
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="strength"> Buff 강도 </param>
        /// <param name="duration"> -1이면 무한지속 </param>
        /// <param name="isStackable"> 중첩 가능 </param>
        public void AddBuff(BuffTypes buff, float strength = 1, float duration = -1, bool isStackable = false)
        {
            if (__immunedBuffs.Contains(buff))
            {
                __Logger.LogF(gameObject, nameof(AddBuff), "__immunedBuffs.Contains(buff) is true.", "buff", buff);
                return;
            }

            var prevBuffActive = CheckBuff(buff);

            if (isStackable)
            {
                if (!__stackableBuffTable.ContainsKey(buff))
                    __stackableBuffTable.Add(buff, new List<Tuple<float, float>>());

                var buffTable = __stackableBuffTable[buff];
                var prevStackCount = buffTable.Count;

                for (int i = 0; i < buffTable.Count; i++)
                {
                    // strength가 큰 순서대로 삽입
                    if (strength >= buffTable[i].Item1)
                    {
                        buffTable.Insert(i, new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration));
                        break;
                    }

                    // 제일 끝에 추가
                    if (i == buffTable.Count - 1)
                        buffTable.Add(new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration));
                }

                if (prevStackCount == 0 && prevStackCount != buffTable.Count && !prevBuffActive)
                {
                    __Logger.LogF(gameObject, nameof(AddBuff), "Stackable-Buff is activated", "buff", buff, "stackCount", buffTable.Count);
                    onBuffActive?.Invoke(buff);
                }
            }
            else
            {
                var tuple = new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration);
                if (!__uniqueBuffTable.ContainsKey(buff))
                {
                    __uniqueBuffTable.Add(buff, tuple);
                    if (!prevBuffActive)
                    {
                        __Logger.LogF(gameObject, nameof(AddBuff), "Unique-Buff is activated", "buff", buff);
                        onBuffActive?.Invoke(buff);
                    }
                }
                else
                {
                    __uniqueBuffTable[buff] = tuple;
                }
            }
        }

        public void AddExternBuff(IBuffContainer buffContainer, BuffTypes buff, float strength = 1, float duration = -1)
        {
            if (__immunedBuffs.Contains(buff))
            {
                __Logger.LogF(gameObject, nameof(AddExternBuff), "__immunedBuffs.Contains(buff) is true.", "buff", buff);
                return;
            }

            var externBuffTable = buffContainer.GetBuffTable();
            Debug.Assert(externBuffTable != null);

            var prevBuffActive = CheckBuff(buff);
            var tuple = new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration);
            if (!externBuffTable.ContainsKey(buff))
            {
                externBuffTable.Add(buff, tuple);
                if (!prevBuffActive)
                {
                    __Logger.LogF(gameObject, nameof(AddExternBuff), "Extern-Buff is activated", "buff", buff);
                    onBuffActive?.Invoke(buff);
                }
            }
            else
            {
                __uniqueBuffTable[buff] = tuple;
            }
        }

        public void RemoveBuff(BuffTypes buff, bool isStackable = false)
        {
            var prevBuffActive = CheckBuff(buff);

            if (isStackable)
            {
                if (!__stackableBuffTable.ContainsKey(buff))
                    return;

                var prevStackCount = __stackableBuffTable[buff].Count;
                __stackableBuffTable[buff].Clear();

                if (prevStackCount != 0 && prevBuffActive && !CheckBuff(buff))
                {
                    __Logger.LogF(gameObject, nameof(RemoveBuff), "Stackable-Buff is deactivated", "buff", buff);
                    onBuffDeactive?.Invoke(buff);
                }
            }
            else
            {
                if (__uniqueBuffTable.Remove(buff) && prevBuffActive && !CheckBuff(buff))
                {
                    __Logger.LogF(gameObject, nameof(RemoveBuff), "Unique-Buff is deactivated", "buff", buff);
                    onBuffDeactive?.Invoke(buff);
                }
            }
        }
        
        public void RemoveExternBuff(IBuffContainer buffContainer, BuffTypes buff)
        {
            var externBuffTable = buffContainer.GetBuffTable();
            Debug.Assert(externBuffTable != null);

            var prevBuffActive = CheckBuff(buff);
            if (externBuffTable.Remove(buff) && prevBuffActive && !CheckBuff(buff))
            {
                __Logger.LogF(gameObject, nameof(RemoveBuff), "Extern-Buff is deactivated", "buff", buff);
                onBuffDeactive?.Invoke(buff);
            }
        }

        PawnBrainController __brain;

        void Awake()
        {
            __brain = GetComponent<PawnBrainController>();
        }

        void Start()
        {
            __brain.onTick += OnTickHandler;
        }

        public void OnTickHandler(float interval)
        {
            foreach (var p in __stackableBuffTable)
            {
                var prevBuffActive = CheckBuff(p.Key);
                var prevStackCount = p.Value.Count;
                for (int i = p.Value.Count - 1; i >= 0; i--)
                {
                    var durationTimeStamp = p.Value[i].Item2;

                    //* 시간이 경관된 buff 삭제
                    if (durationTimeStamp > 0 && durationTimeStamp < Time.time)
                    {
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Stackable-Buff is time-out.", "buff", p.Key, "stackCount", p.Value.Count);
                        p.Value.RemoveAt(i);
                        continue;
                    }
                }

                if (prevStackCount != 0 && p.Value.Count == 0 && prevBuffActive && !CheckBuff(p.Key))
                {
                    __Logger.LogF(gameObject, nameof(OnTickHandler), "Stackable-Buff is deactivated", "buff", p.Key);
                    onBuffDeactive?.Invoke(p.Key);
                }
            }

            for (int i = __uniqueBuffTable.Count - 1; i >= 0; i--)
            {
                var pair = __uniqueBuffTable.ElementAt(i);
                var prevBuffActive = CheckBuff(pair.Key);

                //* 시간이 경관된 buff 삭제
                if (pair.Value.Item2 > 0 && pair.Value.Item2 < Time.time)
                {
                    __Logger.LogF(gameObject, nameof(OnTickHandler), "Unique-Buff is time-out.", "buff", pair.Key);
                    __uniqueBuffTable.Remove(pair.Key);

                    if (prevBuffActive && !CheckBuff(pair.Key))
                    {
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Unique-Buff is deactivated", "buff", pair.Key);
                        onBuffDeactive?.Invoke(pair.Key);
                    }
                }
            }

            foreach (var p in __externBuffTables)
            {
                for (int i = p.Value.Count - 1; i >= 0; i--)
                {
                    var pair = __uniqueBuffTable.ElementAt(i);
                    var prevBuffActive = CheckBuff(pair.Key);

                    //* 시간이 경관된 buff 삭제
                    if (pair.Value.Item2 > 0 && pair.Value.Item2 < Time.time)
                    {
                        __uniqueBuffTable.Remove(pair.Key);
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Extern-Buff is time-out.", "buff", pair.Key);

                        if (prevBuffActive && !CheckBuff(pair.Key))
                        {
                            __Logger.LogF(gameObject, nameof(OnTickHandler), "Extern-Buff is deactivated", "buff", pair.Key);
                            onBuffDeactive?.Invoke(pair.Key);
                        }
                    }
                }
            }
        }
    }
}