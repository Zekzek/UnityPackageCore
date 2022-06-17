using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.MusicMaker
{
    namespace UtiliZek
    {
        public class TimedEventUtil : MonoBehaviour
        {
            public NoteData[] notes;

            public delegate void PlayEvent(float percentComplete, float eventDuration);
            public delegate bool EventReady();
            public static TimedEventUtil Instance { get; private set; }

            private Dictionary<string, TimedEventList> timers;

            private void Awake()
            {
                Instance = this;
                timers = new Dictionary<string, TimedEventList>();
            }

            private void FixedUpdate()
            {
                float modTime = Time.fixedTime % 60;
                foreach (KeyValuePair<string, TimedEventList> pair in timers) {
                    pair.Value.PlayNext(modTime - Time.deltaTime, modTime);
                }
            }

            public void Init(string key, int sequencesPerMinute, bool usePowersOf2)
            {
                if (!timers.ContainsKey(key))
                    timers.Add(key, new TimedEventList(sequencesPerMinute, usePowersOf2));
            }

            public void AddEvent(string key, GameObject gameObject, PlayEvent playEvent, EventReady eventReady)
            {
                if (!timers[key].HasEventFor(gameObject))
                    timers[key].AddEvent(gameObject, playEvent, eventReady);
            }
        }

        class TimedEventList
        {
            private int sequencesPerMinute;
            private bool usePowersOf2;

            private int nextPowerOf2 = 1;
            private float partialCredit = 0;

            private List<TimedEvent> timedEventList;
            private float timeBetweenEvents;
            private int currentIndex = -1;
            private bool active = false;

            public TimedEventList(int sequencesPerMinute, bool usePowersOf2)
            {
                this.sequencesPerMinute = sequencesPerMinute;
                this.usePowersOf2 = usePowersOf2;

                timedEventList = new List<TimedEvent>();
                CalcTimeBetweenEvents();
            }

            public bool HasEventFor(GameObject gameObject)
            {
                return timedEventList.Exists(e => gameObject.Equals(e.GameObject));
            }

            public void AddEvent(GameObject gameObject, TimedEventUtil.PlayEvent playEvent, TimedEventUtil.EventReady eventReady)
            {
                if (!HasEventFor(gameObject)) {
                    timedEventList.Add(new TimedEvent(gameObject, playEvent, eventReady));
                    CalcTimeBetweenEvents();
                }
            }

            public void PlayNext(float begin, float end)
            {
                if (timedEventList.Count == 0)
                    return;

                while (begin > 0) {
                    begin -= timeBetweenEvents;
                    end -= timeBetweenEvents;
                }

                if (end >= 0) {
                    FinishCurrentEvent();
                    partialCredit += ((float)timedEventList.Count) / nextPowerOf2;
                    if (partialCredit >= 1) {
                        partialCredit -= 1;
                        FindNextEvent();
                    }
                } else if (active)
                    timedEventList[currentIndex].PlayEvent((begin + timeBetweenEvents) / timeBetweenEvents, timeBetweenEvents);
            }

            private void FinishCurrentEvent()
            {
                if (active) {
                    timedEventList[currentIndex].PlayEvent(1, timeBetweenEvents);
                }
                active = false;
            }

            private void FindNextEvent()
            {
                //start at the next event and loop all the way around to the current event
                for (int i = 1; i <= timedEventList.Count; i++) {
                    if (timedEventList[(currentIndex + i) % timedEventList.Count].EventReady()) {
                        currentIndex = (currentIndex + i) % timedEventList.Count;
                        timedEventList[currentIndex].PlayEvent(0, timeBetweenEvents);
                        active = true;
                        break;
                    }
                }
            }

            private void CalcTimeBetweenEvents()
            {
                nextPowerOf2 = Mathf.RoundToInt(
                    Mathf.Pow(2,
                        Mathf.Ceil(
                            Mathf.Log(
                                Mathf.Max(1, timedEventList.Count),
                            2)
                        )
                    )
                );
                timeBetweenEvents = 60f / sequencesPerMinute / nextPowerOf2;
            }
        }

        class TimedEvent
        {
            public GameObject GameObject { get; private set; }
            public TimedEventUtil.PlayEvent PlayEvent { get; private set; }
            public TimedEventUtil.EventReady EventReady { get; private set; }

            public TimedEvent(GameObject gameObject, TimedEventUtil.PlayEvent playEvent, TimedEventUtil.EventReady eventReady)
            {
                GameObject = gameObject;
                PlayEvent = playEvent;
                EventReady = eventReady;
            }
        }
    }
}