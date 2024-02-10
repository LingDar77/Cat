namespace Cat.Utillities
{
    interface ICatDriver
    {
        void Drive();
    }

    interface ICatDriver<T1>
    {
        void Drive(T1 p1);
    }

    interface ICatDriver<T1, T2>
    {
        void Drive(T1 p1, T2 p2);
    }

    interface ICatDriver<T1, T2, T3>
    {
        void Drive(T1 p1, T2 p2, T3 p3);
    }
    interface ICatDriver<T1, T2, T3, T4>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4);
    }

    interface ICatDriver<T1, T2, T3, T4, T5>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    }

    interface ICatDriver<T1, T2, T3, T4, T5, T6>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
    }

    interface ICatDriver<T1, T2, T3, T4, T5, T6, T7>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
    }

    interface ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
    }

    interface ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
    }

    interface ICatDriver<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        void Drive(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);
    }

}