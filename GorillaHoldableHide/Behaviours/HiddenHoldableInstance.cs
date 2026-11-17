using BepInEx.Configuration;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DropPositions = BodyDockPositions.DropPositions;

namespace GorillaHoldableHide.Behaviours;

[RequireComponent(typeof(TransferrableObject)), DisallowMultipleComponent]
internal class HiddenHoldableInstance : MonoBehaviour
{
    public HiddenHoldableManager HoldableManager;

    public CosmeticItemInstance ItemInstance;

    private TransferrableObject _transferrableObject;

    private GameObject[] _holdableObjects;

    private bool _initialized;

    private bool _playerFits = true, _isLocalPlayer;

    private bool _categoryFits = true, _isArmCosmetic, _isBackCosmetic, _isChestCosmetic;

    private UnityLayer _hiddenLayer;

    private readonly Dictionary<GameObject, UnityLayer> _originalObjectLayer = [];

    private bool _lastGrabState = false, _forceSetState = true, _sustainVisibility = false, _visibility = true;

    private bool _returning;

    public void Start()
    {
        _initialized = true;
        _transferrableObject = GetComponent<TransferrableObject>();

        _isLocalPlayer = HoldableManager.Rig.isOfflineVRRig;
        var dockPositions = _transferrableObject.dockPositions;
        _isArmCosmetic = dockPositions.HasFlag(DropPositions.LeftArm) && dockPositions.HasFlag(DropPositions.RightArm);
        _isBackCosmetic = dockPositions.HasFlag(DropPositions.LeftBack) && dockPositions.HasFlag(DropPositions.RightBack);
        _isChestCosmetic = dockPositions.HasFlag(DropPositions.Chest);

        _holdableObjects = [.. ItemInstance.allRenderers.Select(renderer => renderer.gameObject).Concat(ItemInstance.allParticles.Select(particle => particle.gameObject)).Distinct()];
        _holdableObjects.ForEach(gameObject => _originalObjectLayer.TryAdd(gameObject, (UnityLayer)gameObject.layer));

        HoldableManager.SubscribeHoldable(this);
        Config_SettingChanged(null, null);
        CheckState();
    }

    public void OnEnable()
    {
        Plugin.Config.SettingChanged += Config_SettingChanged;

        if (_initialized)
        {
            _forceSetState = true;
            HoldableManager.SubscribeHoldable(this);
        }
    }

    public void OnDisable()
    {
        Plugin.Config.SettingChanged -= Config_SettingChanged;

        if (_initialized)
        {
            _forceSetState = true;
            HoldableManager.UnsubscribeHoldable(this);
        }
    }

    public void CheckState()
    {
        bool clearSetState = false;

        bool moving = _transferrableObject.interpDt > 0;
        bool realGrabState = _transferrableObject.InHand();
        _returning = _returning || (!realGrabState && moving);
        bool grabState = _lastGrabState ? (realGrabState || !_returning || moving) : realGrabState;

        if (_forceSetState || _lastGrabState != grabState)
        {
            clearSetState = true;
            _returning = false;
            _sustainVisibility = false;
            _lastGrabState = grabState;
            if (!_sustainVisibility) SetVisibility(grabState);
        }

        if (_forceSetState || (_sustainVisibility && !_visibility))
        {
            clearSetState = true;
            SetVisibility(true);
        }

        if (clearSetState) _forceSetState = false;
    }

    public void SetVisibility(bool isVisible)
    {
        _visibility = isVisible;
        _holdableObjects.ForEach(gameObject =>
        {
            UnityLayer layer = isVisible ? _originalObjectLayer[gameObject] : _hiddenLayer;
            gameObject.SetLayer(layer);
        });
    }

    private void Config_SettingChanged(object sender, SettingChangedEventArgs e) // parameters don't matter here
    {
        _playerFits = (_isLocalPlayer ? Plugin.AffectLocal : Plugin.AffectOthers).Value;
        _categoryFits = _isArmCosmetic ? Plugin.RegisterArm.Value : (_isBackCosmetic ? Plugin.RegisterBack.Value : (_isChestCosmetic && Plugin.RegisterChest.Value));
        _hiddenLayer = Plugin.ShowThirdPerson.Value ? UnityLayer.MirrorOnly : UnityLayer.Bake;
        _sustainVisibility = !_playerFits || !_categoryFits;
        _forceSetState = true;
    }
}
