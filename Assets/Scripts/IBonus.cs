using UnityEngine;

public interface IBonus
{
    BonusType Type { get; }
}
public enum BonusType
{
    Speed,
    Range,
    Health,
    Bombs
}