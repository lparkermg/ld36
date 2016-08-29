using UnityEngine;
using System.Collections;

public class MinionBot : MonoBehaviour {

    public float MaxHp = 100.0f;
    public float CurrentHp = 100.0f;

    public float BaseAttack = 40.0f;
    private float _maxBonusAttack = 20.0f;

    public int CritChance = 50;
    private float _critMultiplier = 2f;

    public float BaseDefence = 10.0f;
    private float _maxBonusDefence = 7.5f;

    private AudioSource _sfxSource;

    //private GameplayManager _gameplayManager;

    public int X;
    public int Y;

    void Start()
    {
        _sfxSource = GetComponent<AudioSource>();
        _sfxSource.volume = GameManager.Instance.SfxVolume / 2;
        //var manager = GameObject.FindGameObjectWithTag("Managers");
        //_gameplayManager = manager.GetComponent<GameplayManager>();
    }

    public void InitBot()
    {
        BaseAttack = Random.Range(15.0f, 50.0f);
        CritChance = Random.Range(10, 85);
        BaseDefence = Random.Range(10f, 35.0f);
    }

    public void UpdatePosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool TakeDamage(float amount)
    {
        var defBonus = Random.Range(0, _maxBonusDefence);
        var def = BaseDefence + defBonus;
        var damage = amount - def;
        if (damage < 0.0f)
            damage = 0.0f;
        CurrentHp -= damage;

        if (CurrentHp <= 0.0f)
        {
            return true;
        }

        return false;
    }

    public float Attack()
    {
        var randomAddedDmg = Random.Range(0.0f, _maxBonusAttack);
        var amount = BaseAttack + randomAddedDmg;
        if(Random.Range(0,100) < CritChance)
        {
            amount = amount * _critMultiplier;
        }
        _sfxSource.clip = GameManager.Instance.GetRandomHitSfxClip();
        _sfxSource.pitch = Random.Range(0.75f, 1.25f);
        _sfxSource.PlayOneShot(_sfxSource.clip);
        return amount;
    }
}
