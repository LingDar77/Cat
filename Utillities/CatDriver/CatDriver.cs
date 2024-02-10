namespace Cat.Utillities
{
    using UnityEngine;
    public abstract class CatDriver : MonoBehaviour, ICatDriver
    {
        public abstract void Drive();
    }
    public abstract class CatDriver<T1> : MonoBehaviour, ICatDriver<T1>
    {
        public abstract void Drive(T1 p1);
    }
    public abstract class CatDriver<T1, T2> : MonoBehaviour, ICatDriver<T1, T2>
    {
        public abstract void Drive(T1 p1, T2 p2);
    }
    public abstract class CatDriver<T1, T2, T3> : MonoBehaviour, ICatDriver<T1, T2, T3>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3);
    }

    public abstract class CatDriver<T1, T2, T3, T4> : MonoBehaviour, ICatDriver<T1, T2, T3, T4>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5, T6> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5, T6>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5, T6, T7> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5, T6, T7>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5, T6, T7, T8> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
    }

    public abstract class CatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MonoBehaviour, ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        public abstract void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);
    }

}