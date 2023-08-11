using JetBrains.Annotations;
using UnInventory.Core.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using UnInventory.Core.Configuration;
using UnInventory.Core.MVC.Controller;
using UnInventory.Core.MVC.View.Components.Slot;

namespace UnInventory.Standard.MVC.Controller
{
    [IsDefaultInventoryCreator]
    [RequireComponent(typeof(ISlotRootComponent))]
    public class SlotNewInputComponent : SlotInputComponent //, IBeginDragHandler, IDragHandler 
    {
        private const float TimerStartAddAmountInHandPerSecond = 0.3f;

        private float _timerAddAmountInHandPerSecond = 0.3f;
        private bool _modeAddAmountInHandPerSecond;
      
        private enum MousePressedModeEnum
        {
            NoMode,
            Left,
            Right
        }

        private static MousePressedModeEnum _mouseMode;

        private static void SetMouseMode(MousePressedModeEnum mode)
        {
            var old = _mouseMode;
            _mouseMode = _mouseMode == MousePressedModeEnum.NoMode ? mode : _mouseMode;
        }
        private static void RemoveMouseMode()
        {
            _mouseMode = MousePressedModeEnum.NoMode;
        }

        protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = obj.AddComponent<EventTrigger>();
            var eventTrigger = new EventTrigger.Entry();
            eventTrigger.eventID = type;
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);
        }

        private void Start() {
            SetEventTriggers(this.gameObject); 
        }

        void SetEventTriggers(GameObject obj) {
            // AddEvent(obj, EventTriggerType.PointerEnter, delegate (BaseEventData eventData){ OnEnter(eventData); });
            // AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            // AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.PointerUp, delegate (BaseEventData eventData){ OnPointerUp(eventData); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate (BaseEventData eventData) { OnBeginDrag(eventData); });
            AddEvent(obj, EventTriggerType.Drag, delegate (BaseEventData eventData) { OnDrag(eventData); });
            AddEvent(obj, EventTriggerType.PointerDown, delegate (BaseEventData eventData){ OnPointerDown(eventData); });
            // AddEvent(obj, EventTriggerType.Select, delegate { OnSelect(obj); });
            // AddEvent(obj, EventTriggerType.Submit, delegate { OnSubmit(obj); });
        }

        void OnEnter(BaseEventData eventData) {
            Debug.Log("fish"); 
        }


        [UsedImplicitly]
        public void Update()
        {
            _timerAddAmountInHandPerSecond -= Time.deltaTime;
            if (_timerAddAmountInHandPerSecond <= 0 
                && _modeAddAmountInHandPerSecond 
                && !Hand.IsEmpty)
            {
                Hand.AddAmountInHand(1);
                _timerAddAmountInHandPerSecond = TimerStartAddAmountInHandPerSecond;
            }
        }
        

        public void OnPointerDown(BaseEventData eventData)
        {
            if ((eventData as PointerEventData).button == PointerEventData.InputButton.Left)
            {
                PressedLeftMouse(eventData as PointerEventData);
            }

            if ((eventData as PointerEventData).button == PointerEventData.InputButton.Right)
            {
                PressedRightMouse(eventData as PointerEventData);
            }
        }

        private void PressedLeftMouse(PointerEventData eventData)
        {
            if (_mouseMode == MousePressedModeEnum.Right)
            {
                return;
            }
            
            if (_mouseMode == MousePressedModeEnum.Left)
            {
                OnPointerUp(eventData);
                return;
            }
            SetMouseMode(MousePressedModeEnum.Left);

            _modeAddAmountInHandPerSecond = true;
            _timerAddAmountInHandPerSecond = TimerStartAddAmountInHandPerSecond;
            Hand.TakeEntityOnPositionInHandTry(SlotComponent.Data, eventData.position);
        }

        private void PressedRightMouse(PointerEventData eventData)
        {
            if (_mouseMode == MousePressedModeEnum.Left)
            {
                return;
            }
            SetMouseMode(MousePressedModeEnum.Right);

            var success = Hand.TakeEntityOnPositionInHandTry(SlotComponent.Data, eventData.position);
            if (success)
            {
                Hand.AddAmountInHandSourcePercent(100);
            }
        }
        
        public void OnPointerUp(BaseEventData _eventData)
        {
            PointerEventData eventData = _eventData as PointerEventData; 
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if(_mouseMode != MousePressedModeEnum.Left) { return;}
                RemoveMouseMode();
                _modeAddAmountInHandPerSecond = false;
                PutInSlotOrUndoTakeHand(eventData);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_mouseMode != MousePressedModeEnum.Right) { return; }
                RemoveMouseMode();
                PutInSlotOrUndoTakeHand(eventData);
            }
        }

        public void OnBeginDrag(BaseEventData eventData)
        {
            if (_mouseMode != MousePressedModeEnum.Left) { return; }
            _modeAddAmountInHandPerSecond = false;
        }
        
        public void OnDrag(BaseEventData _eventData)
        {
            var eventData = _eventData as PointerEventData;
            Hand.PositionSet(eventData.position);
        }
    }
}
