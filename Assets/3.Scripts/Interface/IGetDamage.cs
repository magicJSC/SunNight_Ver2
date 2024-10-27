
public interface IGetDamage
{
    void GetDamage(int damage);
}

public interface IPlayer : IGetDamage
{
}

public interface IMonster : IGetDamage
{
}

public interface IBuilding : IGetDamage
{
}