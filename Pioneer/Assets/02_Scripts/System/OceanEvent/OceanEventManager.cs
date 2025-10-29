using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanEventManager : MonoBehaviour
{
	public static OceanEventManager instance;

	// ���Ƿ� ����
	private List<OceanEventBase> eventList;
	public OceanEventBase currentEvent;

	public Coroutine currentCoroutine;
	private bool isCoroutineStoped = true;

	private void Awake()
	{
		instance = this;

        eventList = new List<OceanEventBase>()
		{
			new OcenaEventNormal(),		// ���
			new OceanEventFog(),		// �Ȱ�
			new OceanEventSiren(),		// ���̷�
			new OceanEventThunder(),    // ����
			new OceanEventWaterBloom()	// ����
		};
		currentEvent = new OcenaEventNormal();
		currentEvent.EventRun();
	}

	// ù���� �ش� �Լ��� ���� �� �� �ȵ˴ϴ�.
	public void EnterDay()
	{
		currentEvent.EventEnd();
		int selectedIndex = Random.Range(0, eventList.Count);
		currentEvent = eventList[selectedIndex];
		eventList.RemoveAt(selectedIndex);

		currentEvent.EventRun();
	}

	public void EnterNight()
	{
		currentEvent.EnterNight();
	}

	public void BeginCoroutine(IEnumerator coroutine)
	{
		IEnumerator m_LocalCoroutine(IEnumerator mCoroutine)
		{
			isCoroutineStoped = false;
			yield return mCoroutine;
			isCoroutineStoped = true;
        }


		if (isCoroutineStoped) currentCoroutine = StartCoroutine(m_LocalCoroutine(coroutine));
	}
}
