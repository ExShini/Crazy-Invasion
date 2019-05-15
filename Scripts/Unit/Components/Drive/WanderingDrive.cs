using System.Collections.Generic;
using Random = UnityEngine.Random;

////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************/
// WanderingDrive
// движетель определяющий бродячее поведение
//
/**********************************************************************************/
public class WanderingDrive : BaseDrive
{
    Point m_pointToMove = null;

    /**********************************************************************************/
    // конструктор
    //
    /**********************************************************************************/
    public WanderingDrive(Unit unitToDrive)
        :base(unitToDrive)
    {
    }

    /**********************************************************************************/
    // основная процессинговая функция движетеля, здесь мы перемещаем юнита
    //
    /**********************************************************************************/
    public override void Update()
    {
        if (!m_driveIsPaused)
        {
            MoveToRandomeBlock();
            UpdateGamePosition();
        }

        UpdateTimers();
    }


    /**********************************************************************************/
    // функция брожения по округе, выбирает рандомный соседний блок и плюхает туда
    //
    /**********************************************************************************/
    protected void MoveToRandomeBlock()
    {
        Point currentPosition = m_unitToDrive.GetGlobalPosition();

        // проверяем - надо ли нам искать новую точку для движения
        bool haveToChouseNewPoint = false;
        if (m_pointToMove == null)
        {
            haveToChouseNewPoint = true;
        }
        else if (currentPosition.IsSamePoint(m_pointToMove))
        {
            haveToChouseNewPoint = true;
        }

        // выбираем рандомный соседний блок
        if (haveToChouseNewPoint)
        {
            // двигаемся в соседний блок
            bool blockIsOk = false;
            Point randomPointInRandomClosestBlock = new Point();
            Point pointToCheck = new Point();


            // симулируем точку в соседнем блоке
            while (!blockIsOk)
            {
                randomPointInRandomClosestBlock.x = 0;
                randomPointInRandomClosestBlock.y = 0;
                Base.DIREC randomDir = Base.GetRandomDirection();

                randomPointInRandomClosestBlock.ShiftPoint(randomDir);
                int sizeOfBlock = MapGenerator.GetInstance().SizeOfBlocks;
                randomPointInRandomClosestBlock.x *= sizeOfBlock;
                randomPointInRandomClosestBlock.y *= sizeOfBlock;

                randomPointInRandomClosestBlock += currentPosition;
                blockIsOk = MapGenerator.GetInstance().CheckCoordinates(randomPointInRandomClosestBlock.x, randomPointInRandomClosestBlock.y);

                // если точка действительно существует (а значит и существует блок для этой точки), выбираем рандомную свободную точку в этом блоке
                if (blockIsOk)
                {
                    Point blockPosition = new Point(randomPointInRandomClosestBlock.x / sizeOfBlock, randomPointInRandomClosestBlock.y / sizeOfBlock);
                    bool pointIsOk = false;

                    while (!pointIsOk)
                    {
                        int xShift = Random.Range(0, sizeOfBlock);
                        int yShift = Random.Range(0, sizeOfBlock);

                        pointToCheck = new Point(blockPosition.x * sizeOfBlock + xShift, blockPosition.y * sizeOfBlock + yShift);
                        pointIsOk = PathFinder.GetInstance().ValidatePathCell(pointToCheck);
                    }
                }
            }

            LinkedList<Point> path = PathFinder.GetInstance().GetWay(currentPosition, pointToCheck);

            // сохраняем первую в списке точку, как точку в соседнем блоке, до которой мы будем пытаться добраться
            m_pointToMove = path.First.Value;
        }

        // двигаемся к заданной точке
        MoveToPoint(m_pointToMove);

    }

    /**********************************************************************************/
    // функция сброса состояния компоненты к дефолтным значениям
    //
    /**********************************************************************************/
    public override void ResetComponent()
    {
        m_pointToMove = null;
        base.ResetComponent();
    }
}

