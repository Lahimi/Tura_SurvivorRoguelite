using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "ScriptableObjects/PowerUp")]
public class SO_PowerUp : ScriptableObject
{
    public enum EffectType 
    { 
        AddMaxHp, AddDefense, AddMoveSpeed, AddMagnetRadius, AddProjectileCount, 
        AddAttackRadius, AddDamagePercent, ReduceAttackInterval, 
        UnlockSmithsSword, UnlockSmallShield, UnlockBoomerang, UnlockMoleMitts, UnlockRocsCape,
        UpgradeEdgeTempering, UpgradeCentrifugalFlux, UpgradeAerodynamicTuning, 
        UpgradeSeismicResonator, UpgradeGaleExtension
    }

    public string displayName;
    public EffectType effectType;
    public float value;
    public int maxStacks = 5;

    public int minLevel;
    public string description;
    public Sprite icon;
}