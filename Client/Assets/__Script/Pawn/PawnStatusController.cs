using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnStatusController : MonoBehaviour
    {
        public Action<PawnStatus> onStatusActive;
        public Action<PawnStatus> onStatusDeactive;

#if UNITY_EDITOR
        public Dictionary<PawnStatus, List<Tuple<float, float>>> StackableTable => __stackableTable;
        public Dictionary<PawnStatus, Tuple<float, float>> UniqueTable => __uniqueTable;
        public HashSet<IStatusContainer> externContainer = new();
#endif

        //* 중첩 가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<PawnStatus, List<Tuple<float, float>>> __stackableTable = new();

        //* 중첩 불가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<PawnStatus, Tuple<float, float>> __uniqueTable = new();

        //* 외부 버프, 중첩 불가능 (Item1: Strength, Item2: Duration TimeStamp)
        readonly Dictionary<IStatusContainer, Dictionary<PawnStatus, Tuple<float, float>>> __externTables = new();

        //* 할당 불가능
        readonly HashSet<PawnStatus> __immunedStatuses = new();

        public bool RegisterExternContainer(IStatusContainer container)
        {
#if UNITY_EDITOR
            externContainer.Add(container);
#endif

            if (!__externTables.ContainsKey(container))
            {
                __externTables.Add(container, container.GetStatusTable());
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnregisterExternContainer(IStatusContainer container)
        {
#if UNITY_EDITOR
            externContainer.Remove(container);
#endif

            __externTables.Remove(container);
        }

        public void SetImmunedStatus(params PawnStatus[] statuses)
        {
            foreach (var s in statuses)
                __immunedStatuses.Add(s);
        }
        
        public bool CheckStatus(PawnStatus status)
        {
            //* 디버프라면 '__externBuffTables' 쪽은 검색에서 제외한다.
            if (status > PawnStatus.__DEBUFF__SEPERATOR__)
                return __uniqueTable.ContainsKey(status) || (__stackableTable.ContainsKey(status) && __stackableTable[status].Count > 0);
            else
                return __uniqueTable.ContainsKey(status) || (__stackableTable.ContainsKey(status) && __stackableTable[status].Count > 0) || __externTables.Any(e => e.Value.ContainsKey(status));
        }

        public float GetStrength(PawnStatus status)
        {
            var ret = 0f;
            if (__uniqueTable.ContainsKey(status))
                ret =  __uniqueTable[status].Item1;
            if (__stackableTable.ContainsKey(status) && __stackableTable[status].Count > 0)
                ret = Mathf.Max(ret, __stackableTable[status].First().Item1);

            //* 디버프가 아닌 경우에만 '__externBuffTables'도 검색한다.
            if (status < PawnStatus.__DEBUFF__SEPERATOR__ && __externTables.Count > 0 && __externTables.Any(e => e.Value.ContainsKey(status)))
                ret = Mathf.Max(ret, __externTables.Max(e => e.Value[status].Item1));

            return ret;
        }
        
        public void AddStatus(PawnStatus status, float strength = 1, float duration = -1, bool isStackable = false)
        {
            if (__immunedStatuses.Contains(status))
            {
                __Logger.LogF(gameObject, nameof(AddStatus), "__immunedBuffs.Contains() is true.", "status", status);
                return;
            }

            var prevValue = CheckStatus(status);

            if (isStackable)
            {
                if (!__stackableTable.ContainsKey(status))
                    __stackableTable.Add(status, new List<Tuple<float, float>>());

                var currTable = __stackableTable[status];
                var prevStackCount = currTable.Count;

                for (int i = 0; i < currTable.Count; i++)
                {
                    //* strength가 큰 순서대로 삽입
                    if (strength >= currTable[i].Item1)
                    {
                        currTable.Insert(i, new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration));
                        break;
                    }

                    //* strength가 제일 작아서 끝에 추가
                    if (i == currTable.Count - 1)
                        currTable.Add(new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration));
                }

                if (prevStackCount == 0 && prevStackCount != currTable.Count && !prevValue)
                {
                    __Logger.LogF(gameObject, nameof(AddStatus), "Stackable-Status is activated", "status", status, "stackCount", currTable.Count);
                    onStatusActive?.Invoke(status);
                }
            }
            else
            {
                var tuple = new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration);
                if (!__uniqueTable.ContainsKey(status))
                {
                    __uniqueTable.Add(status, tuple);
                    if (!prevValue)
                    {
                        __Logger.LogF(gameObject, nameof(AddStatus), "Unique-Status is activated", "status", status);
                        onStatusActive?.Invoke(status);
                    }
                }
                else
                {
                    __uniqueTable[status] = tuple;
                }
            }
        }

        public void AddExternStatus(IStatusContainer container, PawnStatus status, float strength = 1, float duration = -1)
        {
            if (__immunedStatuses.Contains(status))
            {
                __Logger.LogF(gameObject, nameof(AddExternStatus), "__immunedBuffs.Contains(buff) is true.", "status", status);
                return;
            }

            var externTable = container.GetStatusTable();
            Debug.Assert(externTable != null);

            var prevValue = CheckStatus(status);
            var tuple = new Tuple<float, float>(strength, duration < 0 ? duration : Time.time + duration);
            if (!externTable.ContainsKey(status))
            {
                externTable.Add(status, tuple);
                if (!prevValue)
                {
                    __Logger.LogF(gameObject, nameof(AddExternStatus), "Extern-Status is activated", "status", status);
                    onStatusActive?.Invoke(status);
                }
            }
            else
            {
                __uniqueTable[status] = tuple;
            }
        }

        public void RemoveStatus(PawnStatus status, bool isStackable = false)
        {
            var prevValue = CheckStatus(status);

            if (isStackable)
            {
                if (!__stackableTable.ContainsKey(status))
                    return;

                var prevStackCount = __stackableTable[status].Count;
                __stackableTable[status].Clear();

                if (prevStackCount != 0 && prevValue && !CheckStatus(status))
                {
                    __Logger.LogF(gameObject, nameof(RemoveStatus), "Stackable-Status is deactivated", "status", status);
                    onStatusDeactive?.Invoke(status);
                }
            }
            else
            {
                if (__uniqueTable.Remove(status) && prevValue && !CheckStatus(status))
                {
                    __Logger.LogF(gameObject, nameof(RemoveStatus), "Unique-Status is deactivated", "status", status);
                    onStatusDeactive?.Invoke(status);
                }
            }
        }
        
        public void RemoveExternStatus(IStatusContainer container, PawnStatus status)
        {
            var externTable = container.GetStatusTable();
            Debug.Assert(externTable != null);

            var prevValue = CheckStatus(status);
            if (externTable.Remove(status) && prevValue && !CheckStatus(status))
            {
                __Logger.LogF(gameObject, nameof(RemoveStatus), "Extern-Status is deactivated", "status", status);
                onStatusDeactive?.Invoke(status);
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
            foreach (var t in __stackableTable)
            {
                var prevValue = CheckStatus(t.Key);
                var prevStackCount = t.Value.Count;
                for (int i = t.Value.Count - 1; i >= 0; i--)
                {
                    var timeStamp = t.Value[i].Item2;

                    //* 시간이 경관된 Status 삭제
                    if (timeStamp > 0 && timeStamp < Time.time)
                    {
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Stackable-Status is time-out.", "status", t.Key, "stackCount", t.Value.Count);
                        t.Value.RemoveAt(i);
                        continue;
                    }
                }

                if (prevStackCount != 0 && t.Value.Count == 0 && prevValue && !CheckStatus(t.Key))
                {
                    __Logger.LogF(gameObject, nameof(OnTickHandler), "Stackable-Status is deactivated", "status", t.Key);
                    onStatusDeactive?.Invoke(t.Key);
                }
            }

            for (int i = __uniqueTable.Count - 1; i >= 0; i--)
            {
                var pair = __uniqueTable.ElementAt(i);
                var prevValue = CheckStatus(pair.Key);
                var timeStamp = pair.Value.Item2;

                //* 시간이 경관된 buff 삭제
                if (timeStamp > 0 && timeStamp < Time.time)
                {
                    __Logger.LogF(gameObject, nameof(OnTickHandler), "Unique-Status is time-out.", "status", pair.Key);
                    __uniqueTable.Remove(pair.Key);

                    if (prevValue && !CheckStatus(pair.Key))
                    {
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Unique-Status is deactivated", "status", pair.Key);
                        onStatusDeactive?.Invoke(pair.Key);
                    }
                }
            }

            foreach (var p in __externTables)
            {
                for (int i = p.Value.Count - 1; i >= 0; i--)
                {
                    var pair = p.Value.ElementAt(i);
                    var prevValue = CheckStatus(pair.Key);
                    var timeStamp = pair.Value.Item2;

                    //* 시간이 경관된 Status 삭제
                    if (timeStamp > 0 && timeStamp < Time.time)
                    {
                        p.Value.Remove(pair.Key);
                        __Logger.LogF(gameObject, nameof(OnTickHandler), "Extern-Status is time-out.", "status", pair.Key);

                        if (prevValue && !CheckStatus(pair.Key))
                        {
                            __Logger.LogF(gameObject, nameof(OnTickHandler), "Extern-Status is deactivated", "status", pair.Key);
                            onStatusDeactive?.Invoke(pair.Key);
                        }
                    }
                }
            }
        }
    }
}