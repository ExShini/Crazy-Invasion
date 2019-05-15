using UnityEngine;

/**********************************************************************************/
// EmptyDrive класс
// движетель пустышка - используется с объектами у которых нет механики перемещения
// ничего не делает
//
/**********************************************************************************/
class EmptyDrive : IDrive
{
    public event PositionUpdateEvent PositionUpdate;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public EmptyDrive()
    {
        // nothing to do
    }

    public virtual void ResetComponent()
    {
        // nothing to do
    }

    public virtual void SetTargetToMove(GameObject target)
    {
        // nothing to do
    }

    public virtual void StartMoving()
    {
        // nothing to do
    }

    public virtual void StopMoving()
    {
        // nothing to do
    }

    public virtual void Update()
    {
        // nothing to do
    }
}
