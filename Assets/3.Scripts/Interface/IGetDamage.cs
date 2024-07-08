
public interface IGetDamage
{
    void GetDamge(float damage);
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