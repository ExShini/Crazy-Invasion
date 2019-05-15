/**********************************************************************************/
// EmptyRadar класс
// радар пустышка - ничего не видет, никого ни о чём не извещает
//
/**********************************************************************************/
class EmptyRadar : IRadar
{
    public event RadarDataEvent RadarUpdate;
    public event GOEvent TargetToMove;

    public void PositionUpdate(Point position)
    {
        // nothing to do
    }

    public void ResetComponent()
    {
        // nothing to do
    }

    public void Update()
    {
        // nothing to do
    }
}

