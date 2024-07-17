
public interface IGetDamage
{
    void GetDamage(int damage);
}

public interface IPlayer : IGetDamage,IDie
{

}

public interface IMonster : IGetDamage,IDie
{

}

public interface IBuilding : IGetDamage
{

}

public interface IDie
{
    public void Die();
}