// for uGUI(from 4.6)
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5)

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UniRx.Examples
{
    // "Sample12Scene" 씬의 Canavas에서 설정합니다.
    public class Sample12_ReactiveProperty : MonoBehaviour
    {
        public Button MyButton;
        public Toggle MyToggle;
        public InputField MyInput;
        public Text MyText;
        public Slider MySlider;

        // SpecializedReactiveProperty를 통해 인스펙터에서 모니터링/수정이 가능하도록 합니다.
        public IntReactiveProperty IntRxProp = new IntReactiveProperty();

        Enemy enemy = new Enemy(1000);

        void Start()
        {
            // UnityEvent를 Observable 하도록 합니다.
            // (shortcut, MyButton.OnClickAsObservable())
            MyButton.onClick.AsObservable().Subscribe(_ => enemy.CurrentHp.Value -= 99);

            // 토글, 입력 등을 Observable로 (OnValueChangedAsObservable은 구독 시 isOn 값을 제공하기 위한 도우미입니다)
            // SubscribeToInteractable은 UniRx.UI 확장 메서드로, .interactable = x 와 동일합니다.
            MyToggle.OnValueChangedAsObservable().SubscribeToInteractable(MyButton);

            // InputField의 입력은 1초의 지연 후에 보여지도록 합니다.
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            MyInput.OnValueChangedAsObservable()
#else
            MyInput.OnValueChangeAsObservable()
#endif
                .Where(x => x != null)
                .Delay(TimeSpan.FromSeconds(1))
                .SubscribeToText(MyText); // SubscribeToText는 UniRx.UI의 Extension Method입니다.

            // 슬라이더의 float value를 사람이 보기에 편하도록 convert 합니다.
            MySlider.OnValueChangedAsObservable()
                .SubscribeToText(MyText, x => Math.Round(x, 2).ToString());

            // from RxProp, CurrentHp changing(Button Click) is observable
            // RxProp에서 CurrentHp 변경(버튼 클릭)이 관찰 가능합니다.
            enemy.CurrentHp.SubscribeToText(MyText);
            enemy.IsDead.Where(isDead => isDead == true)
                .Subscribe(_ =>
                {
                    MyToggle.interactable = MyButton.interactable = false;
                });

            // initial text:)
            IntRxProp.SubscribeToText(MyText);
        }
    }

    // Reactive Notification Model
    public class Enemy
    {
        public IReactiveProperty<long> CurrentHp { get; private set; }

        public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }

        public Enemy(int initialHp)
        {
            // Declarative Property
            CurrentHp = new ReactiveProperty<long>(initialHp);
            IsDead = CurrentHp.Select(x => x <= 0).ToReactiveProperty();
        }
    }
}

#endif