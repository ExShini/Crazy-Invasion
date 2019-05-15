
/**********************************************************************************/
// EmptyArmor класс
// броня пустышка - используется с объектами у которых нет механики повреждения
//
/**********************************************************************************/
class EmptyArmor : IArmor
{
    public event DestructionEvent UnitIsDown;

    public void ResetComponent()
    {
        // nothing to do
    }

    public void SetHealth(int health)
    {
        // nothing to do
    }

    public void TakeDamage(DamageData damage)
    {
        // nothing to do
    }

    public void Update()
    {
        // nothing to do
    }
}
