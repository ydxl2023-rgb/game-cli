using System;
using System.Collections.Generic;

public class XTimer
{
	const int TimerMaxInit = 16;

	class Timer
	{
		internal int sn;
		internal int cycle;
		internal int deadline;
		internal Func<int, bool> handler;
		internal bool delete;
		internal List<Timer> container;
	}

	class Wheel
	{
		internal static int dialScale = 256;
		internal int head;
		internal List<Timer>[] vecDial;
		internal int dialSize;
		internal int timeRange;
		internal Wheel nextWheel;

		internal Wheel(int nDialSize)
		{
			dialSize = nDialSize;
			timeRange = nDialSize * dialScale;
			head = 0;
			vecDial = new List<Timer>[dialScale];

			for (int i = 0; i < dialScale; ++i)
			{
				vecDial[i] = new List<Timer>(TimerMaxInit);
			}
		}
		internal List<Timer> NextDial()
		{
			return vecDial[head++];
		}

		internal void Add(int delay, Timer tm)
		{
			var container = vecDial[(head + (delay - (dialSize - jiffiesMsec)) / dialSize) % dialScale];
			container.Add(tm);
			tm.container = container;
		}
	}

	static int nextSn = 0;
	static int jiffiesMsec = 20;

	static float jiffiesSec = jiffiesMsec * .001f;

	static Wheel[] wheels;
	static float pileSecs;
	static float nowTime;
	static Dictionary<int, Timer> mapSnTimer;
	static List<Timer> executeTimers;

	static int IntPow(int n, int m)
	{
		int ret = 1;
		for (int i = 0; i < m; ++i)
			ret *= n;

		return ret;
	}

	static void InnerAdd(int deadline, Timer tm)
	{
		tm.deadline = deadline;

		int delay = Math.Max(0, deadline - Now());
		Wheel suitableWheel = wheels[wheels.Length - 1];

		for (int i = 0; i < wheels.Length; ++i)
		{
			var wheel = wheels[i];
			if (delay < wheel.timeRange)
			{
				suitableWheel = wheel;
				break;
			}
		}

		suitableWheel.Add(delay, tm);
	}

	static void InnerDel(Timer tm)
	{
		InnerDel(tm, true);
	}

	static void InnerDel(Timer tm, bool removeFromMap)
	{
		tm.delete = true;

		if (tm.container != null)
		{
			tm.container.Remove(tm);
			tm.container = null;
		}

		if (removeFromMap)
			mapSnTimer.Remove(tm.sn);
	}

	static int Now()
	{
		return (int)(nowTime * 1000);
	}

	public static void Tick(float deltaTime)
	{
		if (executeTimers == null)
			return;

		nowTime += deltaTime;
		pileSecs += deltaTime;

		int cycle = 0;
		while (pileSecs >= jiffiesSec)
		{
			pileSecs -= jiffiesSec;
			cycle++;
		}
		for (int i = 0; i < cycle; ++i)
		{
			var timers = wheels[0].NextDial();
			for (int j = 0; j < timers.Count; ++j)
			{
				var tm = timers[j];
				executeTimers.Add(tm);
			}
			timers.Clear();

			for (int j = 0; j < wheels.Length; ++j)
			{
				var wheel = wheels[j];
				if (wheel.head == Wheel.dialScale)
				{
					wheel.head = 0;
					if (wheel.nextWheel != null)
					{
						var tms = wheel.nextWheel.NextDial();
						for (int k = 0; k < tms.Count; ++k)
						{
							var tm = tms[k];
							if (tm.delete)
							{
								mapSnTimer.Remove(tm.sn);
							}
							else
							{
								InnerAdd(tm.deadline, tm);
							}
						}
						tms.Clear();
					}
				}
				else
				{
					break;
				}
			}
		}

		for (int n = 0; n < executeTimers.Count; n++)
		{
			var tm = executeTimers[n];
			// if callback return true or any value!=false, 
			// re-add timer to new wheel
			if (!tm.delete && tm.handler(tm.sn) && tm.cycle > 0)
			{
				InnerAdd(Now() + tm.cycle, tm);
			}
			else
			{
				mapSnTimer.Remove(tm.sn);
			}
		}

		executeTimers.Clear();
	}

	static bool inited = false;

	public static bool Init()
	{
		if (inited)
			return false;

		inited = true;

		wheels = new Wheel[4];
		for (int i = 0; i < 4; ++i)
		{
			wheels[i] = new Wheel(jiffiesMsec * IntPow(Wheel.dialScale, i));
			if (i > 0)
			{
				wheels[i - 1].nextWheel = wheels[i];
			}
		}

		mapSnTimer = new Dictionary<int, Timer>();
		executeTimers = new List<Timer>(TimerMaxInit);

		return true;
	}

	static int FetchSn()
	{
		return ++nextSn;
	}

	internal static void Del(int sn)
	{
		if (mapSnTimer.TryGetValue(sn, out Timer tm))
		{
			InnerDel(tm);
		}
	}

	public static int Add(int delay, int cycle, Func<int, bool> handler)
	{
		Timer tm = new Timer()
		{
			sn = FetchSn(),
			cycle = cycle,
			handler = handler
		};

		mapSnTimer[tm.sn] = tm;

		// add new timer
		InnerAdd(Now() + delay, tm);
		return tm.sn;
	}


	public static int Add(int delay, Action<int> handler)
	{
		return Add(delay, 0, (int sn) => {
			handler(sn);
			return false;
		});
	}

	public static void Delete(int nId)
	{
		Del(nId);
	}

	public static bool DeleteAll()
	{
		if (mapSnTimer == null)
			return false;

		foreach (var t in mapSnTimer)
		{
			InnerDel(t.Value, false);
		}

		mapSnTimer.Clear();

		return true;
	}
}

#pragma warning restore IDE0044 // ����ֻ�����η�

