namespace TUI.Utillities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Serialization;
    using Type = System.Type;
    using Enum = System.Enum;
    using SerializableAttribute = System.SerializableAttribute;

    public enum CallState
    {
        Off = 0,
        EditorAndRuntime = 1,
        RuntimeOnly = 2,
    }

    /// <summary>
    /// Represents a base class for handling dynamic method invocation.
    /// </summary>
    public abstract class InvokableCallBase
    {
        protected string thisAssembly = null;
        protected string thisMethodName = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected InvokableCallBase() { }

        /// <summary>
        /// Constructor that initializes the invokable action based on a target and method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="function">The method to be invoked.</param>
        protected InvokableCallBase(object target, MethodInfo function)
        {
            if (function == null)
                throw new System.ArgumentNullException(nameof(function));

            if (function.IsStatic)
            {
                if (target != null)
                    throw new System.ArgumentException("target must be null");
            }
            else
            {
                if (target == null)
                    throw new System.ArgumentNullException("target");
            }

            thisAssembly = target.GetType().AssemblyQualifiedName;
            thisMethodName = function.Name;
        }

        /// <summary>
        /// Invokes the encapsulated method using the provided arguments array.
        /// </summary>
        /// <param name="args">Array of arguments to pass to the method.
        /// The array is expected to have a length equal to the number of type arguments.</param>
        public abstract void Invoke(object[] args);

        /// <summary>
        /// Throws an exception if the provided argument is not of the expected type.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="arg">The argument to check.</param>
        /// <param name="thisMethodName">The name of the method being invoked.</param>
        /// <param name="thisAssembly">The name of the assembly containing the invoked method.</param>
        protected static void ThrowOnInvalidArg<T>(object arg, string thisMethodName, string thisAssembly)
        {
            if (arg != null && !(arg is T))
                throw new System.ArgumentException(string.Format("Passed argument 'arg' on method '{0}' for target '{1}' is of the wrong type. Type:{2} Expected:{3}", thisMethodName, thisAssembly, arg.GetType(), typeof(T)));
        }

        /// <summary>
        /// Throws an exception if the provided argument is not of the expected type.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="arg">The argument to check.</param>
        protected static void ThrowOnInvalidArg<T>(object arg)
        {
            if (arg != null && !(arg is T))
                throw new System.ArgumentException(string.Format("Passed argument 'args[0]' is of the wrong type. Type:{0} Expected:{1}", arg.GetType(), typeof(T)));
        }

        /// <summary>
        /// Determines if the provided delegate is invokable based on its target type.
        /// </summary>
        /// <param name="delegate">The delegate to check.</param>
        /// <returns><c>true</c> if the delegate is invokable; otherwise, <c>false</c>.</returns>
        protected static bool AllowInvoke(System.Delegate @delegate)
        {
            var target = @delegate.Target;

            // static
            if (target == null)
                return true;

            // UnityEngine object
            var unityObj = target as Object;
            if (!ReferenceEquals(unityObj, null))
                return unityObj != null;

            // Normal object
            return true;
        }

        /// <summary>
        /// Determines if the provided method on the target object matches the encapsulated method.
        /// </summary>
        /// <param name="targetObj">The target object containing the method.</param>
        /// <param name="method">The method to compare.</param>
        /// <returns><c>true</c> if the method on the target object matches the encapsulated method; otherwise, <c>false</c>.</returns>
        public abstract bool Find(object targetObj, MethodInfo method);
    }

    internal class Helper
    {
        internal static string TidyAssemblyTypeName(string assemblyTypeName)
        {
            if (string.IsNullOrEmpty(assemblyTypeName))
                return assemblyTypeName;

            int min = int.MaxValue;
            int i = assemblyTypeName.IndexOf(", Version=");
            if (i != -1)
                min = Mathf.Min(i, min);
            i = assemblyTypeName.IndexOf(", Culture=");
            if (i != -1)
                min = Mathf.Min(i, min);
            i = assemblyTypeName.IndexOf(", PublicKeyToken=");
            if (i != -1)
                min = Mathf.Min(i, min);

            if (min != int.MaxValue)
                assemblyTypeName = assemblyTypeName.Substring(0, min);

            i = assemblyTypeName.IndexOf(", UnityEngine.");
            if (i != -1 && assemblyTypeName.EndsWith("Module"))
                assemblyTypeName = assemblyTypeName.Substring(0, i) + ", UnityEngine";
            return assemblyTypeName;
        }
    }

    public class InvokableCall : InvokableCallBase
    {
        /// <summary>
        /// Encapsulated UnityAction delegate for invoking methods without parameters.
        /// </summary>
        private event UnityAction Delegate;

        /// <summary>
        /// Constructs the invoker based on a target object and a method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, theFunction);
        }

        /// <summary>
        /// Constructs the invoker based on an existing UnityAction delegate.
        /// </summary>
        /// <param name="action">The UnityAction delegate.</param>
        public InvokableCall(UnityAction action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (AllowInvoke(Delegate))
                Delegate();
        }

        /// <summary>
        /// Invokes the encapsulated method without any arguments.
        /// </summary>
        public void Invoke()
        {
            if (AllowInvoke(Delegate))
                Delegate();
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0> : InvokableCallBase
    {
        /// <summary>
        /// Encapsulated UnityAction delegate for invoking methods with a single parameter of type T0.
        /// </summary>
        protected event UnityAction<T0> Delegate;

        /// <summary>
        /// Constructs the invoker based on a target object and a method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0>)System.Delegate.CreateDelegate(typeof(UnityAction<T0>), target, theFunction);
        }

        /// <summary>
        /// Constructs the invoker based on an existing UnityAction delegate with a single generic parameter.
        /// </summary>
        /// <param name="action">The UnityAction delegate.</param>
        public InvokableCall(UnityAction<T0> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 1)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 1");

            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);

            if (!AllowInvoke(Delegate))
                return;

            Delegate((T0)args[0]);
        }

        /// <summary>
        /// Invokes the encapsulated method with a single argument of type T0.
        /// </summary>
        /// <param name="args0">The argument of type T0.</param>
        public virtual void Invoke(T0 args0)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1> : InvokableCallBase
    {
        /// <summary>
        /// Encapsulated UnityAction delegate for invoking methods with two parameters of types T0 and T1.
        /// </summary>
        protected event UnityAction<T0, T1> Delegate;

        /// <summary>
        /// Constructs the invoker based on a target object and a method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1>), target, theFunction);
        }

        /// <summary>
        /// Constructs the invoker based on an existing UnityAction delegate with two generic parameters.
        /// </summary>
        /// <param name="action">The UnityAction delegate.</param>
        public InvokableCall(UnityAction<T0, T1> action)
        {
            Delegate += action;
        }

        /// <summary>
        /// Invokes the encapsulated method using the provided arguments array.
        /// </summary>
        /// <param name="args">Array of arguments to pass to the method. The array is expected to have two items of types T0 and T1.</param>
        public override void Invoke(object[] args)
        {
            if (args.Length != 2)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 2");

            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);

            if (!AllowInvoke(Delegate)) return;
            Delegate((T0)args[0], (T1)args[1]);
        }

        /// <summary>
        /// Invokes the encapsulated method with two arguments of types T0 and T1.
        /// </summary>
        /// <param name="args0">The first argument of type T0.</param>
        /// <param name="args1">The second argument of type T1.</param>
        public virtual void Invoke(T0 args0, T1 args1)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2> : InvokableCallBase
    {
        /// <summary>
        /// Encapsulated UnityAction delegate for invoking methods with three parameters of types T0, T1, and T2.
        /// </summary>
        protected event UnityAction<T0, T1, T2> Delegate;

        /// <summary>
        /// Constructs the invoker based on a target object and a method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2>), target, theFunction);
        }

        /// <summary>
        /// Constructs the invoker based on an existing UnityAction delegate with three generic parameters.
        /// </summary>
        /// <param name="action">The UnityAction delegate.</param>
        public InvokableCall(UnityAction<T0, T1, T2> action)
        {
            Delegate += action;
        }

        /// <summary>
        /// Invokes the encapsulated method using the provided arguments array.
        /// </summary>
        /// <param name="args">Array of arguments to pass to the method. The array is expected to have three items of types T0, T1, and T2.</param>
        public override void Invoke(object[] args)
        {
            if (args.Length != 3)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 3");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);

            if (!AllowInvoke(Delegate)) return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2]);
        }

        /// <summary>
        /// Invokes the encapsulated method with three arguments of types T0, T1, and T2.
        /// </summary>
        /// <param name="args0">The first argument of type T0.</param>
        /// <param name="args1">The second argument of type T1.</param>
        /// <param name="args2">The third argument of type T2.</param>
        public virtual void Invoke(T0 args0, T1 args1, T2 args2)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3> : InvokableCallBase
    {
        /// <summary>
        /// Encapsulated UnityAction delegate for invoking methods with four parameters of types T0, T1, T2, and T3.
        /// </summary>
        protected event UnityAction<T0, T1, T2, T3> Delegate;

        /// <summary>
        /// Constructs the invoker based on a target object and a method.
        /// </summary>
        /// <param name="target">The target object containing the method.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3>), target, theFunction);
        }

        /// <summary>
        /// Constructs the invoker based on an existing UnityAction delegate with four generic parameters.
        /// </summary>
        /// <param name="action">The UnityAction delegate.</param>
        public InvokableCall(UnityAction<T0, T1, T2, T3> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 4)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 4");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);

            if (!AllowInvoke(Delegate)) return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]);
        }

        /// <summary>
        /// Invokes the encapsulated method with four arguments of types T0, T1, T2, and T3.
        /// </summary>
        /// <param name="args0">The first argument of type T0.</param>
        /// <param name="args1">The second argument of type T1.</param>
        /// <param name="args2">The third argument of type T2.</param>
        /// <param name="args3">The fourth argument of type T3.</param>
        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4> : InvokableCallBase
    {
        /// <summary>
        /// Represents the delegate event for this invokable event.
        /// </summary>
        protected event UnityAction<T0, T1, T2, T3, T4> Delegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokableCall{T0, T1, T2, T3, T4}"/> class using the target object and method.
        /// </summary>
        /// <param name="target">The target object that contains the method to be invoked.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4>), target, theFunction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokableCall{T0, T1, T2, T3, T4}"/> class using the provided action delegate.
        /// </summary>
        /// <param name="action">The delegate action to be invoked.</param>
        public InvokableCall(UnityAction<T0, T1, T2, T3, T4> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 5)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 5");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]);
        }

        /// <summary>
        /// Invokes the delegate with the provided arguments.
        /// </summary>
        /// <param name="args0">The first argument of type <see cref="T0"/>.</param>
        /// <param name="args1">The second argument of type <see cref="T1"/>.</param>
        /// <param name="args2">The third argument of type <see cref="T2"/>.</param>
        /// <param name="args3">The fourth argument of type <see cref="T3"/>.</param>
        /// <param name="args4">The fifth argument of type <see cref="T4"/>.</param>
        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4);
        }

        /// <summary>
        /// Determines whether the delegate targets the provided object and method.
        /// </summary>
        /// <param name="targetObj">The target object to check.</param>
        /// <param name="method">The method to check.</param>
        /// <returns><c>true</c> if the delegate targets the specified object and method; otherwise, <c>false</c>.</returns>
        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4, T5> : InvokableCallBase
    {
        /// <summary>
        /// Represents the delegate event for this invokable event.
        /// </summary>
        protected event UnityAction<T0, T1, T2, T3, T4, T5> Delegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokableCall{T0, T1, T2, T3, T4, T5}"/> class using the target object and method.
        /// </summary>
        /// <param name="target">The target object that contains the method to be invoked.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4, T5>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4, T5>), target, theFunction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokableCall{T0, T1, T2, T3, T4, T5}"/> class using the provided action delegate.
        /// </summary>
        /// <param name="action">The delegate action to be invoked.</param>
        public InvokableCall(UnityAction<T0, T1, T2, T3, T4, T5> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 6)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 6");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T5>(args[5], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5]);
        }

        /// <summary>
        /// Invokes the delegate with the provided arguments.
        /// </summary>
        /// <param name="args0">The first argument of type <see cref="T0"/>.</param>
        /// <param name="args1">The second argument of type <see cref="T1"/>.</param>
        /// <param name="args2">The third argument of type <see cref="T2"/>.</param>
        /// <param name="args3">The fourth argument of type <see cref="T3"/>.</param>
        /// <param name="args4">The fifth argument of type <see cref="T4"/>.</param>
        /// <param name="args5">The sixth argument of type <see cref="T5"/>.</param>
        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4, args5);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4, T5, T6> : InvokableCallBase
    {
        protected event UnityAction<T0, T1, T2, T3, T4, T5, T6> Delegate;

        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4, T5, T6>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4, T5, T6>), target, theFunction);
        }

        public InvokableCall(UnityAction<T0, T1, T2, T3, T4, T5, T6> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 7)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 7");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T5>(args[5], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T6>(args[6], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6]);
        }

        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4, args5, args6);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7> : InvokableCallBase
    {
        protected event UnityAction<T0, T1, T2, T3, T4, T5, T6, T7> Delegate;

        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4, T5, T6, T7>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7>), target, theFunction);
        }

        public InvokableCall(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 8)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 8");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T5>(args[5], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T6>(args[6], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T7>(args[7], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7]);
        }

        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4, args5, args6, args7);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8> : InvokableCallBase
    {
        protected event UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> Delegate;

        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8>), target, theFunction);
        }

        public InvokableCall(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 9)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 9");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T5>(args[5], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T6>(args[6], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T7>(args[7], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T8>(args[8], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8]);
        }

        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7, T8 args8)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4, args5, args6, args7, args8);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : InvokableCallBase
    {
        protected event UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> Delegate;

        public InvokableCall(object target, MethodInfo theFunction) : base(target, theFunction)
        {
            Delegate += (UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)System.Delegate.CreateDelegate(typeof(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>), target, theFunction);
        }

        public InvokableCall(UnityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            Delegate += action;
        }

        public override void Invoke(object[] args)
        {
            if (args.Length != 10)
                throw new System.ArgumentException("Passed argument 'args' is invalid size. Expected size is 10");
            ThrowOnInvalidArg<T0>(args[0], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T1>(args[1], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T2>(args[2], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T3>(args[3], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T4>(args[4], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T5>(args[5], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T6>(args[6], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T7>(args[7], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T8>(args[8], thisMethodName, thisAssembly);
            ThrowOnInvalidArg<T9>(args[9], thisMethodName, thisAssembly);
            if (!AllowInvoke(Delegate))
                return;
            Delegate((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9]);
        }

        public virtual void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7, T8 args8, T9 args9)
        {
            if (AllowInvoke(Delegate))
                Delegate(args0, args1, args2, args3, args4, args5, args6, args7, args8, args9);
        }

        public override bool Find(object targetObj, MethodInfo method)
        {
            return Delegate.Target == targetObj && Delegate.Method.Equals(method);
        }
    }

    public class UpdatableInvokableCall<T0> : InvokableCall<T0>
    {
        private readonly object[] m_Args = new object[1];

        private bool m_IsEventDefined = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableInvokableCall{T0}"/> class using the target object, method, a flag indicating if the event is defined, and an argument value.
        /// </summary>
        /// <param name="target">The target object that contains the method to be invoked.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        /// <param name="isEventDefined">Flag indicating if the event is defined.</param>
        /// <param name="arg">The initial argument value for the invokable event.</param>
        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg) : base(target, theFunction)
        {
            m_Args[0] = arg;
            m_IsEventDefined = isEventDefined;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableInvokableCall{T0}"/> class using the target object, method, and a flag indicating if the event is defined.
        /// </summary>
        /// <param name="target">The target object that contains the method to be invoked.</param>
        /// <param name="theFunction">The method to be invoked.</param>
        /// <param name="isEventDefined">Flag indicating if the event is defined.</param>
        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
            }

            base.Invoke((T0)m_Args[0]);
        }
    }

    public class UpdatableInvokableCall<T0, T1> : InvokableCall<T0, T1>
    {
        private readonly object[] m_Args = new object[2];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2> : InvokableCall<T0, T1, T2>
    {
        private readonly object[] m_Args = new object[3];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3> : InvokableCall<T0, T1, T2, T3>
    {
        private readonly object[] m_Args = new object[4];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4> : InvokableCall<T0, T1, T2, T3, T4>
    {
        private readonly object[] m_Args = new object[5];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4, T5> : InvokableCall<T0, T1, T2, T3, T4, T5>
    {
        private readonly object[] m_Args = new object[6];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_Args[5] = arg5;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
                m_Args[5] = args5;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4], (T5)m_Args[5]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4, T5, T6> : InvokableCall<T0, T1, T2, T3, T4, T5, T6>
    {
        private readonly object[] m_Args = new object[7];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_Args[5] = arg5;
            m_Args[6] = arg6;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
                m_Args[5] = args5;
                m_Args[6] = args6;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4], (T5)m_Args[5], (T6)m_Args[6]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4, T5, T6, T7> : InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        private readonly object[] m_Args = new object[8];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_Args[5] = arg5;
            m_Args[6] = arg6;
            m_Args[7] = arg7;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
                m_Args[5] = args5;
                m_Args[6] = args6;
                m_Args[7] = args7;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4], (T5)m_Args[5], (T6)m_Args[6], (T7)m_Args[7]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8> : InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    {
        private readonly object[] m_Args = new object[9];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_Args[5] = arg5;
            m_Args[6] = arg6;
            m_Args[7] = arg7;
            m_Args[8] = arg8;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7, T8 args8)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
                m_Args[5] = args5;
                m_Args[6] = args6;
                m_Args[7] = args7;
                m_Args[8] = args8;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4], (T5)m_Args[5], (T6)m_Args[6], (T7)m_Args[7], (T8)m_Args[8]);
        }
    }

    public class UpdatableInvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : InvokableCall<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        private readonly object[] m_Args = new object[10];

        private bool m_IsEventDefined = false;

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) : base(target, theFunction)
        {
            m_Args[0] = arg0;
            m_Args[1] = arg1;
            m_Args[2] = arg2;
            m_Args[3] = arg3;
            m_Args[4] = arg4;
            m_Args[5] = arg5;
            m_Args[6] = arg6;
            m_Args[7] = arg7;
            m_Args[8] = arg8;
            m_Args[9] = arg9;
            m_IsEventDefined = isEventDefined;
        }

        public UpdatableInvokableCall(Object target, MethodInfo theFunction, bool isEventDefined) : base(target, theFunction)
        {
            m_IsEventDefined = isEventDefined;
        }

        public override void Invoke(object[] args)
        {
            if (m_IsEventDefined)
                for (int i = 0; i < args.Length; i++)
                    m_Args[i] = args[i];
            base.Invoke(m_Args);
        }

        public override void Invoke(T0 args0, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5, T6 args6, T7 args7, T8 args8, T9 args9)
        {
            if (m_IsEventDefined)
            {
                m_Args[0] = args0;
                m_Args[1] = args1;
                m_Args[2] = args2;
                m_Args[3] = args3;
                m_Args[4] = args4;
                m_Args[5] = args5;
                m_Args[6] = args6;
                m_Args[7] = args7;
                m_Args[8] = args8;
                m_Args[9] = args9;
            }

            base.Invoke((T0)m_Args[0], (T1)m_Args[1], (T2)m_Args[2], (T3)m_Args[3], (T4)m_Args[4], (T5)m_Args[5], (T6)m_Args[6], (T7)m_Args[7], (T8)m_Args[8], (T9)m_Args[9]);
        }
    }


    [System.Serializable]
    class ArgumentCache : ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("objectArgument")]
        [SerializeField]
        private Object m_ObjectArgument;
        [SerializeField]
        [FormerlySerializedAs("objectArgumentAssemblyTypeName")]
        private string m_ObjectArgumentAssemblyTypeName;
        [FormerlySerializedAs("intArgument")]
        [SerializeField]
        private int m_IntArgument;
        [FormerlySerializedAs("floatArgument")]
        [SerializeField]
        private float m_FloatArgument;
        [SerializeField]
        [FormerlySerializedAs("stringArgument")]
        private string m_StringArgument;
        [SerializeField]
        private bool m_BoolArgument;
        [SerializeField]
        [FormerlySerializedAs("enumArgument")]
        private System.Enum m_EnumArgument;
        [SerializeField]
        [FormerlySerializedAs("vector2Argument")]
        private Vector2 m_Vector2Argument;
        [SerializeField]
        [FormerlySerializedAs("vector2IntArgument")]
        private Vector2Int m_Vector2IntArgument;
        [SerializeField]
        [FormerlySerializedAs("vector3Argument")]
        private Vector3 m_Vector3Argument;
        [SerializeField]
        [FormerlySerializedAs("vector3IntArgument")]
        private Vector3Int m_Vector3IntArgument;
        [SerializeField]
        [FormerlySerializedAs("vector4Argument")]
        private Vector4 m_Vector4Argument;
        [SerializeField]
        [FormerlySerializedAs("layerMaskArgument")]
        private LayerMask m_LayerMaskArgument;
        [SerializeField]
        [FormerlySerializedAs("colorArgument")]
        private Color m_ColorArgument;
        [SerializeField]
        [FormerlySerializedAs("quaternionArgument")]
        private Quaternion m_QuaternionArgument;

        /// <summary>
        /// Gets or sets the Unity object argument.
        /// </summary>
        public Object unityObjectArgument
        {
            get => m_ObjectArgument;
            set
            {
                m_ObjectArgument = value;
                m_ObjectArgumentAssemblyTypeName = value != null ? value.GetType().AssemblyQualifiedName : string.Empty;
            }
        }

        /// <summary>
        /// Gets the assembly qualified type name of the Unity object argument.
        /// </summary>
        public string unityObjectArgumentAssemblyTypeName
        {
            get => m_ObjectArgumentAssemblyTypeName;
        }

        public int intArgument { get => m_IntArgument; set => m_IntArgument = value; }
        public float floatArgument { get => m_FloatArgument; set => m_FloatArgument = value; }
        public string stringArgument { get => m_StringArgument; set => m_StringArgument = value; }
        public bool boolArgument { get => m_BoolArgument; set => m_BoolArgument = value; }

        public System.Enum enumArgument
        {
            get => m_EnumArgument;
            set
            {
                m_EnumArgument = value;
                m_ObjectArgumentAssemblyTypeName = value == null ? string.Empty : value.GetType().AssemblyQualifiedName;
            }
        }

        public Vector2 vector2Argument
        {
            get => m_Vector2Argument;
            set
            {
                m_Vector2Argument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Vector2).AssemblyQualifiedName;
            }
        }
        public Vector2Int vector2IntArgument
        {
            get => m_Vector2IntArgument;
            set
            {
                m_Vector2IntArgument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Vector2Int).AssemblyQualifiedName;
            }
        }

        public Vector3 vector3Argument
        {
            get => m_Vector3Argument;
            set
            {
                m_Vector3Argument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Vector3).AssemblyQualifiedName;
            }
        }
        public Vector3Int vector3IntArgument
        {
            get => m_Vector3IntArgument;
            set
            {
                m_Vector3IntArgument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Vector3Int).AssemblyQualifiedName;
            }
        }

        public Vector4 vector4Argument
        {
            get => m_Vector4Argument;
            set
            {
                m_Vector4Argument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Vector4).AssemblyQualifiedName;
            }
        }
        public LayerMask layerMaskArgument
        {
            get => m_LayerMaskArgument;
            set
            {
                m_LayerMaskArgument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(LayerMask).AssemblyQualifiedName;
            }
        }

        public Color colorArgument
        {
            get => m_ColorArgument;
            set
            {
                m_ColorArgument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Color).AssemblyQualifiedName;
            }
        }

        public Quaternion quaternionArgument
        {
            get => m_QuaternionArgument;
            set
            {
                m_QuaternionArgument = value;
                m_ObjectArgumentAssemblyTypeName = typeof(Quaternion).AssemblyQualifiedName;
            }
        }

        public void OnBeforeSerialize()
        {
            m_ObjectArgumentAssemblyTypeName = Helper.TidyAssemblyTypeName(m_ObjectArgumentAssemblyTypeName);
        }

        public void OnAfterDeserialize()
        {
            m_ObjectArgumentAssemblyTypeName = Helper.TidyAssemblyTypeName(m_ObjectArgumentAssemblyTypeName);
        }
    }

    [System.Serializable]
    class PersistentCall : ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("instance")]
        [SerializeField]
        private Object m_Target;

        [SerializeField]
        private string m_TargetAssemblyTypeName;

        [FormerlySerializedAs("methodName")]
        [SerializeField]
        private string m_MethodName;

        [FormerlySerializedAs("arguments")]
        [SerializeField]
        private List<ArgumentCache> m_Arguments = new List<ArgumentCache>();

        [SerializeField]
        [FormerlySerializedAs("enabled")]
        [FormerlySerializedAs("m_Enabled")]
        private CallState m_CallState = CallState.RuntimeOnly;


        [SerializeField]
        [FormerlySerializedAs("modes")]
        private List<ListenerMode> m_Modes = new List<ListenerMode>();

        public Object target
        {
            get { return m_Target; }
        }

        public string targetAssemblyTypeName
        {
            get
            {
                // Reconstruct TargetAssemblyTypeName from target if it's not present, for ex., when upgrading project
                if (string.IsNullOrEmpty(m_TargetAssemblyTypeName) && m_Target != null)
                {
                    m_TargetAssemblyTypeName = Helper.TidyAssemblyTypeName(m_Target.GetType().AssemblyQualifiedName);
                }

                return m_TargetAssemblyTypeName;
            }
        }

        public string methodName
        {
            get { return m_MethodName; }
        }

        public List<ArgumentCache> arguments => m_Arguments;

        public CallState callState
        {
            get => m_CallState;
            set => m_CallState = value;
        }

        public List<ListenerMode> modes
        {
            get => m_Modes;
            set => m_Modes = value;
        }

        public bool IsValid()
        {
            // We need to use the same logic found in PersistentCallCollection.cpp, IsPersistentCallValid
            return !string.IsNullOrEmpty(targetAssemblyTypeName) && !string.IsNullOrEmpty(methodName);
        }

        public InvokableCallBase GetRuntimeCall(EventBase theEvent, object[] parameters)
        {
            if (m_CallState == CallState.RuntimeOnly && !Application.isPlaying)
                return null;
            if (m_CallState == CallState.Off || theEvent == null)
                return null;
            if (m_Modes.Count == 0)
                return null;

            var method = theEvent.FindMethod(this);
            if (method == null)
                return null;

            if (!method.IsStatic && target == null)
                return null;

            if (m_Modes.Count != 1) return GetInvokableCall(target, method, m_Modes, m_Arguments.ToArray(), parameters);
            switch (m_Modes[0])
            {
                case ListenerMode.Void:
                    return new InvokableCall(target, method);
                case ListenerMode.Int:
                    return new UpdatableInvokableCall<int>(target, method, false, m_Arguments[0].intArgument);
                case ListenerMode.Float:
                    return new UpdatableInvokableCall<float>(target, method, false, m_Arguments[0].floatArgument);
                case ListenerMode.String:
                    return new UpdatableInvokableCall<string>(target, method, false, m_Arguments[0].stringArgument);
                case ListenerMode.Bool:
                    return new UpdatableInvokableCall<bool>(target, method, false, m_Arguments[0].boolArgument);
                case ListenerMode.Vector2:
                    return new UpdatableInvokableCall<Vector2>(target, method, false, m_Arguments[0].vector2Argument);
                case ListenerMode.Vector3:
                    return new UpdatableInvokableCall<Vector3>(target, method, false, m_Arguments[0].vector3Argument);
                case ListenerMode.Vector2Int:
                    return new UpdatableInvokableCall<Vector2Int>(target, method, false, m_Arguments[0].vector2IntArgument);
                case ListenerMode.Vector3Int:
                    return new UpdatableInvokableCall<Vector3Int>(target, method, false, m_Arguments[0].vector3IntArgument);
                case ListenerMode.Vector4:
                    return new UpdatableInvokableCall<Vector4>(target, method, false, m_Arguments[0].vector4Argument);
                case ListenerMode.LayerMask:
                    return new UpdatableInvokableCall<LayerMask>(target, method, false, m_Arguments[0].layerMaskArgument);
                case ListenerMode.Color:
                    return new UpdatableInvokableCall<Color>(target, method, false, m_Arguments[0].colorArgument);
                case ListenerMode.Quaternion:
                    return new UpdatableInvokableCall<Quaternion>(target, method, false, m_Arguments[0].quaternionArgument);
                case ListenerMode.Object:
                    return GetObjectCall(target, method, m_Arguments[0]);
                default:
                    break;
            }

            return GetInvokableCall(target, method, m_Modes, m_Arguments.ToArray(), parameters);
        }

        private class CachedInvokableCall<T> : InvokableCall<T>
        {
            private readonly T m_Arg1;

            public CachedInvokableCall(Object target, MethodInfo theFunction, T argument)
                : base(target, theFunction)
            {
                m_Arg1 = argument;
            }

            public override void Invoke(object[] args)
            {
                base.Invoke(m_Arg1);
            }

            public override void Invoke(T arg0)
            {
                base.Invoke(m_Arg1);
            }
        }

        private InvokableCallBase GetObjectCall(Object target, MethodInfo method, ArgumentCache arguments)
        {

            Type type = typeof(Object);
            if (!string.IsNullOrEmpty(arguments.unityObjectArgumentAssemblyTypeName))
                type = Type.GetType(arguments.unityObjectArgumentAssemblyTypeName, false) ?? typeof(Object);

            var generic = typeof(CachedInvokableCall<>);
            var specific = generic.MakeGenericType(type);
            var ci = specific.GetConstructor(new[] { typeof(Object), typeof(MethodInfo), type });

            var castedObject = arguments.unityObjectArgument;
            if (castedObject != null && !type.IsAssignableFrom(castedObject.GetType()))
                castedObject = null;

            // need to pass explicit null here!
            try
            {
                return ci.Invoke(new object[] { target, method, castedObject }) as InvokableCallBase;
            }
            catch
            {
                Debug.LogError($"[EventsPro] There was an error invoking {method}! One or more UnityEvents may be missing parameters!");

                return null;
            }
        }

        public void RegisterPersistentListener(Object ttarget, string methodName)
        {
            m_Target = ttarget;
            m_MethodName = methodName;
        }

        public void RegisterPersistentListener(Object ttarget, Type targetType, string mmethodName)
        {
            m_Target = ttarget;
            m_TargetAssemblyTypeName = Helper.TidyAssemblyTypeName(targetType.AssemblyQualifiedName);
            m_MethodName = mmethodName;
        }

        public void UnregisterPersistentListener()
        {
            m_MethodName = string.Empty;
            m_Target = null;
            m_TargetAssemblyTypeName = string.Empty;
        }

        public void OnBeforeSerialize()
        {
            m_TargetAssemblyTypeName = Helper.TidyAssemblyTypeName(m_TargetAssemblyTypeName);
        }

        public void OnAfterDeserialize()
        {
            m_TargetAssemblyTypeName = Helper.TidyAssemblyTypeName(m_TargetAssemblyTypeName);
        }

        private InvokableCallBase GetInvokableCall(Object target, MethodInfo method, List<ListenerMode> modes, ArgumentCache[] arguments, object[] informedParameters)
        {
            Type[] typeArguments = new Type[arguments.Length];
            Type[] types = new Type[arguments.Length + 3];
            object[] parameters = new object[arguments.Length + 3];

            types[0] = typeof(Object);
            types[1] = typeof(MethodInfo);
            types[2] = typeof(bool);

            parameters[0] = target;
            parameters[1] = method;
            parameters[2] = false;

            for (int i = 0; i < typeArguments.Length; i++)
            {
                typeArguments[i] = Type.GetType(arguments[i].unityObjectArgumentAssemblyTypeName, false) ?? typeof(Object);
                types[i + 3] = typeArguments[i];

                if (modes[i] == ListenerMode.EventDefined)
                {
                    parameters[2] = true;
                    parameters[i + 3] = informedParameters[i];
                }
                else
                {
                    parameters[i + 3] = GetValue(typeArguments[i], arguments[i]);
                }
            }

#if UNITY_2020_2_OR_NEWER
            Type invokableType = typeArguments.Length switch
            {
                1 => typeof(UpdatableInvokableCall<>),
                2 => typeof(UpdatableInvokableCall<,>),
                3 => typeof(UpdatableInvokableCall<,,>),
                4 => typeof(UpdatableInvokableCall<,,,>),
                5 => typeof(UpdatableInvokableCall<,,,,>),
                6 => typeof(UpdatableInvokableCall<,,,,,>),
                7 => typeof(UpdatableInvokableCall<,,,,,,>),
                8 => typeof(UpdatableInvokableCall<,,,,,,,>),
                9 => typeof(UpdatableInvokableCall<,,,,,,,,>),
                10 => typeof(UpdatableInvokableCall<,,,,,,,,,>),
                _ => typeof(InvokableCall)
            };
#else
			Type invokableType;

			switch (typeArguments.Length)
			{
				case 1:
					invokableType = typeof(UpdatableInvokableCall<>);
					break;
				case 2:
					invokableType = typeof(UpdatableInvokableCall<,>);
					break;
				case 3:
					invokableType = typeof(UpdatableInvokableCall<,,>);
					break;
				case 4:
					invokableType = typeof(UpdatableInvokableCall<,,,>);
					break;
				case 5:
					invokableType = typeof(UpdatableInvokableCall<,,,,>);
					break;
				case 6:
					invokableType = typeof(UpdatableInvokableCall<,,,,,>);
					break;
				case 7:
					invokableType = typeof(UpdatableInvokableCall<,,,,,,>);
					break;
				case 8:
					invokableType = typeof(UpdatableInvokableCall<,,,,,,,>);
					break;
				case 9:
					invokableType = typeof(UpdatableInvokableCall<,,,,,,,,>);
					break;
				case 10:
					invokableType = typeof(UpdatableInvokableCall<,,,,,,,,,>);
					break;
				default:
					invokableType = typeof(EP_InvokableCall);
					break;
			}
#endif

            ConstructorInfo constructor = invokableType.MakeGenericType(typeArguments).GetConstructor(types);

            try
            {
                return constructor == null ? null : constructor.Invoke(parameters) as InvokableCallBase;
            }
            catch
            {
                Debug.LogError($"[EventsPro] There was an error invoking {method}! One or more UnityEvents may be missing parameters!");

                return null;
            }
        }

        private T CastObject<T>(object objectInput)
        {
            return (T)objectInput;
        }

        private object GetValue(Type type, ArgumentCache argument)
        {
            if (type == typeof(int))
                return argument.intArgument;
            else if (type.IsEnum)
                return Enum.GetValues(type).GetValue(argument.intArgument);

            if (type == typeof(float))
                return argument.floatArgument;
            else if (type == typeof(string))
                return argument.stringArgument;
            else if (type == typeof(bool))
                return argument.boolArgument;
            else if (type == typeof(Vector2))
                return argument.vector2Argument;
            else if (type == typeof(Vector3))
                return argument.vector3Argument;
            else if (type == typeof(Vector2Int))
                return argument.vector2IntArgument;
            else if (type == typeof(Vector3Int))
                return argument.vector3IntArgument;
            else if (type == typeof(Vector4))
                return argument.vector4Argument;
            else if (type == typeof(LayerMask))
                return argument.layerMaskArgument;
            else if (type == typeof(Color))
                return argument.colorArgument;
            else if (type == typeof(Quaternion))
                return argument.quaternionArgument;

            if (argument.unityObjectArgument != null && !type.IsAssignableFrom(argument.unityObjectArgument.GetType()))
                return null;

            return argument.unityObjectArgument;
        }


    }

    [Serializable]
    class PersistentCallGroup
    {
        [SerializeField]
        [FormerlySerializedAs("m_Listeners")]
        private List<PersistentCall> m_Calls;

        public int Count => m_Calls.Count;

        public PersistentCallGroup()
        {
            m_Calls = new List<PersistentCall>();
        }

        public PersistentCall GetListener(int index)
        {
            return m_Calls[index];
        }

        public IEnumerable<PersistentCall> GetListeners()
        {
            return m_Calls;
        }

        public void AddListener()
        {
            m_Calls.Add(new PersistentCall());
        }

        public void AddListener(PersistentCall call)
        {
            m_Calls.Add(call);
        }

        public void RemoveListener(int index)
        {
            m_Calls.RemoveAt(index);
        }

        public void Clear()
        {
            m_Calls.Clear();
        }

        public void RegisterEventPersistentListener(int index, Object targetObj, Type targetObjType, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.EventDefined);
        }

        public void RegisterVoidPersistentListener(int index, Object targetObj, Type targetObjType, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.Void);
        }

        public void RegisterObjectPersistentListener(int index, Object targetObj, Type targetObjType, Object argument, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.Object);
            listener.arguments.Add(new ArgumentCache() { unityObjectArgument = argument });
        }

        public void RegisterIntPersistentListener(int index, Object targetObj, Type targetObjType, int argument, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.Int);
            listener.arguments.Add(new ArgumentCache() { intArgument = argument });
        }

        public void RegisterFloatPersistentListener(int index, Object targetObj, Type targetObjType, float argument, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.Float);
            listener.arguments.Add(new ArgumentCache() { floatArgument = argument });
        }

        public void RegisterStringPersistentListener(int index, Object targetObj, Type targetObjType, string argument, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.String);
            listener.arguments.Add(new ArgumentCache() { stringArgument = argument });
        }

        public void RegisterBoolPersistentListener(int index, Object targetObj, Type targetObjType, bool argument, string methodName)
        {
            var listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, targetObjType, methodName);
            listener.modes.Add(ListenerMode.Bool);
            listener.arguments.Add(new ArgumentCache() { boolArgument = argument });
        }

        public void RegisterEnumPersistentListener(int index, Object targetObj, Enum argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Enum);
            listener.arguments.Add(new ArgumentCache() { enumArgument = argument });
        }

        public void RegisterVector2PersistentListener(int index, Object targetObj, Vector2 argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Vector2);
            listener.arguments.Add(new ArgumentCache() { vector2Argument = argument });
        }

        public void RegisterVector2IntPersistentListener(int index, Object targetObj, Vector2Int argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Vector2Int);
            listener.arguments.Add(new ArgumentCache() { vector2IntArgument = argument });
        }


        public void RegisterVector3PersistentListener(int index, Object targetObj, Vector3 argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Vector3);
            listener.arguments.Add(new ArgumentCache() { vector3Argument = argument });
        }

        public void RegisterVector3IntPersistentListener(int index, Object targetObj, Vector3Int argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Vector3Int);
            listener.arguments.Add(new ArgumentCache() { vector3IntArgument = argument });
        }

        public void RegisterVector4PersistentListener(int index, Object targetObj, Vector4 argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Vector4);
            listener.arguments.Add(new ArgumentCache() { vector4Argument = argument });
        }

        public void RegisterLayerMaskPersistentListener(int index, Object targetObj, LayerMask argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.LayerMask);
            listener.arguments.Add(new ArgumentCache() { layerMaskArgument = argument });
        }

        public void RegisterColorPersistentListener(int index, Object targetObj, Color argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Color);
            listener.arguments.Add(new ArgumentCache() { colorArgument = argument });
        }

        public void RegisterQuaternionPersistentListener(int index, Object targetObj, Quaternion argument, string methodName)
        {
            PersistentCall listener = GetListener(index);
            listener.RegisterPersistentListener(targetObj, methodName);
            listener.modes.Add(ListenerMode.Quaternion);
            listener.arguments.Add(new ArgumentCache() { quaternionArgument = argument });
        }

        public void UnregisterPersistentListener(int index)
        {
            GetListener(index).UnregisterPersistentListener();
        }

        public void RemoveListeners(Object target, string methodName)
        {
            List<PersistentCall> persistentCallList = m_Calls.Where(t => t.target == target && t.methodName == methodName).ToList();
            m_Calls.RemoveAll(new System.Predicate<PersistentCall>(persistentCallList.Contains));
        }

        public void Initialize(InvokableCallList invokeableList, EventBase unityEventBase, object[] parameters)
        {
            foreach (var persistentCall in m_Calls)
            {
                if (!persistentCall.IsValid())
                    continue;
                var call = persistentCall.GetRuntimeCall(unityEventBase, parameters);
                if (call != null)
                    invokeableList.AddPersistentInvokableCall(call);

            }
        }
    }

    class InvokableCallList
    {
        private readonly List<InvokableCallBase> m_PersistentCalls = new List<InvokableCallBase>();
        private readonly List<InvokableCallBase> m_RuntimeCalls = new List<InvokableCallBase>();

        private readonly List<InvokableCallBase> m_ExecutingCalls = new List<InvokableCallBase>();

        private bool m_NeedsUpdate = true;

        public int Count => m_PersistentCalls.Count + m_RuntimeCalls.Count;

        public void AddPersistentInvokableCall(InvokableCallBase call)
        {
            m_PersistentCalls.Add(call);
            m_NeedsUpdate = true;
        }

        public void AddListener(InvokableCallBase call)
        {
            m_RuntimeCalls.Add(call);
            m_NeedsUpdate = true;
        }

        public void RemoveListener(object targetObj, MethodInfo method)
        {
            List<InvokableCallBase> baseInvokableCallList = new List<InvokableCallBase>();
            foreach (InvokableCallBase t in m_RuntimeCalls)
            {
                if (t.Find(targetObj, method))
                    baseInvokableCallList.Add(t);
            }
            m_RuntimeCalls.RemoveAll(new System.Predicate<InvokableCallBase>(baseInvokableCallList.Contains));
            m_NeedsUpdate = true;
        }

        public void Clear()
        {
            m_RuntimeCalls.Clear();
            m_NeedsUpdate = true;
        }

        public void ClearPersistent()
        {
            m_PersistentCalls.Clear();
            m_NeedsUpdate = true;
        }

        public List<InvokableCallBase> PrepareInvoke()
        {
            if (!m_NeedsUpdate) return m_ExecutingCalls;

            m_ExecutingCalls.Clear();
            m_ExecutingCalls.AddRange(m_PersistentCalls);
            m_ExecutingCalls.AddRange(m_RuntimeCalls);
            m_NeedsUpdate = false;

            return m_ExecutingCalls;
        }

        public void Invoke(object[] parameters)
        {
            if (m_NeedsUpdate)
            {
                m_ExecutingCalls.Clear();
                m_ExecutingCalls.AddRange(m_PersistentCalls);
                m_ExecutingCalls.AddRange(m_RuntimeCalls);
                m_NeedsUpdate = false;
            }

            foreach (InvokableCallBase t in m_ExecutingCalls)
                t.Invoke(parameters);
        }
    }

    [Serializable]
    public abstract class EventBase : ISerializationCallbackReceiver
    {
        private InvokableCallList m_Calls;

        [SerializeField]
        [FormerlySerializedAs("m_PersistentListeners")]
        private PersistentCallGroup m_PersistentCalls = new PersistentCallGroup();

        private bool m_CallsDirty = true;

        protected EventBase()
        {

        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DirtyPersistentCalls();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            DirtyPersistentCalls();
        }

        protected MethodInfo FindMethod_Impl(string name, object targetObj)
        {
            return FindMethod_Impl(name, targetObj.GetType());
        }

        protected abstract MethodInfo FindMethod_Impl(object targetObj, string name);

        internal abstract InvokableCallBase GetDelegate(object target, MethodInfo theFunction);

        internal MethodInfo FindMethod(PersistentCall call)
        {
            Type[] argumentTypes = new Type[call.arguments.Count];

            for (int i = 0; i < argumentTypes.Length; i++)
                argumentTypes[i] = Type.GetType(call.arguments[i].unityObjectArgumentAssemblyTypeName, false) ?? typeof(Object);

            return GetValidMethodInfo(call.target, call.methodName, argumentTypes);
        }

        internal MethodInfo FindMethod(object listener, string name, ListenerMode mode, Type argumentType)
        {
#if UNITY_2020_2_OR_NEWER
            return mode switch
            {
                ListenerMode.EventDefined => FindMethod_Impl(listener, name),
                ListenerMode.Void => GetValidMethodInfo(listener, name, new Type[0]),
                ListenerMode.Object => GetValidMethodInfo(listener, name,
                    new Type[1] { argumentType ?? typeof(Object) }),
                ListenerMode.Int => GetValidMethodInfo(listener, name, new Type[1] { typeof(int) }),
                ListenerMode.Float => GetValidMethodInfo(listener, name, new Type[1] { typeof(float) }),
                ListenerMode.String => GetValidMethodInfo(listener, name, new Type[1] { typeof(string) }),
                ListenerMode.Bool => GetValidMethodInfo(listener, name, new Type[1] { typeof(bool) }),
                ListenerMode.Enum => GetValidMethodInfo(listener, name, new Type[1] { typeof(Enum) }),
                _ => null
            };
#else
			MethodInfo result;

			switch (mode)
			{
				case EP_PersistentListenerMode.EventDefined:
					result = FindMethod_Impl(listener, name);
					break;
				case EP_PersistentListenerMode.Void:
					result = GetValidMethodInfo(listener, name, new Type[0]);
					break;
				case EP_PersistentListenerMode.Object:
					result = GetValidMethodInfo(listener, name, new Type[1] { argumentType ?? typeof(Object) });
					break;
				case EP_PersistentListenerMode.Int:
					result = GetValidMethodInfo(listener, name, new Type[1] { typeof(int) });
					break;
				case EP_PersistentListenerMode.Float:
					result = GetValidMethodInfo(listener, name, new Type[1] { typeof(float) });
					break;
				case EP_PersistentListenerMode.String:
					result = GetValidMethodInfo(listener, name, new Type[1] { typeof(string) });
					break;
				case EP_PersistentListenerMode.Bool:
					result = GetValidMethodInfo(listener, name, new Type[1] { typeof(bool) });
					break;
				case EP_PersistentListenerMode.Enum:
					result = GetValidMethodInfo(listener, name, new Type[1] { typeof(Enum) });
					break;
				default:
					result = null;
					break;
			}

			return result;
#endif
        }


        internal MethodInfo FindMethod(string name, Type listenerType, PersistentListenerMode mode, Type argumentType)
        {
            switch (mode)
            {
                case PersistentListenerMode.EventDefined:
                    return FindMethod_Impl(name, listenerType);
                case PersistentListenerMode.Void:
                    return GetValidMethodInfo(listenerType, name, new Type[0]);
                case PersistentListenerMode.Float:
                    return GetValidMethodInfo(listenerType, name, new[] { typeof(float) });
                case PersistentListenerMode.Int:
                    return GetValidMethodInfo(listenerType, name, new[] { typeof(int) });
                case PersistentListenerMode.Bool:
                    return GetValidMethodInfo(listenerType, name, new[] { typeof(bool) });
                case PersistentListenerMode.String:
                    return GetValidMethodInfo(listenerType, name, new[] { typeof(string) });
                case PersistentListenerMode.Object:
                    return GetValidMethodInfo(listenerType, name, new[] { argumentType ?? typeof(Object) });
                default:
                    return null;
            }
        }

        public MethodInfo FindMethod(object listener, string name, ListenerMode mode, Type[] argumentTypes = null)
        {
            if (mode == ListenerMode.EventDefined)
                return FindMethod_Impl(listener, name);

            return GetValidMethodInfo(listener, name, argumentTypes);
        }

        /// <summary>
        ///   <para>Get the number of registered persistent listeners.</para>
        /// </summary>
        public int GetPersistentEventCount()
        {
            return m_PersistentCalls.Count;
        }

        /// <summary>
        ///   <para>Get the target component of the listener at index index.</para>
        /// </summary>
        /// <param name="index">Index of the listener to query.</param>
        public Object GetPersistentTarget(int index)
        {
            var listener = m_PersistentCalls.GetListener(index);
            return listener?.target;
        }

        /// <summary>
        ///   <para>Get the target method name of the listener at index index.</para>
        /// </summary>
        /// <param name="index">Index of the listener to query.</param>
        public string GetPersistentMethodName(int index)
        {
            var listener = m_PersistentCalls.GetListener(index);
            return listener != null ? listener.methodName : string.Empty;
        }

        private void DirtyPersistentCalls()
        {
            m_Calls?.ClearPersistent();
            m_CallsDirty = true;
        }

        private void RebuildPersistentCallsIfNeeded(object[] parameters)
        {
            if (!m_CallsDirty) return;

            m_PersistentCalls.Initialize(Calls, this, parameters);
            m_CallsDirty = false;
        }

        /// <summary>
        ///   <para>Modify the execution state of a persistent listener.</para>
        /// </summary>
        /// <param name="index">Index of the listener to query.</param>
        /// <param name="state">State to set.</param>
        public void SetPersistentListenerState(int index, CallState state)
        {
            PersistentCall listener = m_PersistentCalls.GetListener(index);
            if (listener != null)
                listener.callState = state;

            DirtyPersistentCalls();
        }

        public CallState GetPersistentListenerState(int index)
        {
            if (index < 0 || index > m_PersistentCalls.Count)
                throw new System.IndexOutOfRangeException($"Index {index} is out of range of the {GetPersistentEventCount()} persistent listeners.");
            return m_PersistentCalls.GetListener(index).callState;
        }

        protected void AddListener(object targetObj, MethodInfo method)
        {
            Calls.AddListener(GetDelegate(targetObj, method));
        }

        internal void AddCall(InvokableCallBase call)
        {
            Calls.AddListener(call);
        }

        protected void RemoveListener(object targetObj, MethodInfo method)
        {
            Calls.RemoveListener(targetObj, method);
        }

        /// <summary>
        /// Remove all listeners from the event.
        /// </summary>
        public void RemoveAllListeners()
        {
            Calls.Clear();
        }

        protected List<InvokableCallBase> PrepareInvoke(params object[] parameters)
        {
            RebuildPersistentCallsIfNeeded(parameters);
            return Calls.PrepareInvoke();
        }

        protected void Invoke(object[] parameters)
        {
            List<InvokableCallBase> calls = PrepareInvoke(parameters);

            for (var i = 0; i < calls.Count; i++)
                calls[i].Invoke(parameters);
        }

        public override string ToString()
        {
            return base.ToString() + " " + GetType().FullName;
        }

        // Find a valid method that can be bound to an event with a given name
        public static MethodInfo GetValidMethodInfo(object obj, string functionName, Type[] argumentTypes)
        {
            return GetValidMethodInfo(obj.GetType(), functionName, argumentTypes);
        }

        /// <summary>
        ///   <para>Given an object, function name, and a list of argument types; find the method that matches.</para>
        /// </summary>
        /// <remarks>
        /// We need to make sure the Arguments are sane. When using the Type.DefaultBinder like we are above,
        /// it is possible to receive a method that takes a System.Object enve though we requested a float, int or bool.
        /// This can be an issue when the user changes the signature of a function that he had already set up via inspector.
        /// When changing a float parameter to a System.Object the getMethod would still bind to the cahnged version, but
        /// the PersistentListenerMode would still be kept as Float.</remarks>
        /// <param name="obj">Object to search for the method.</param>
        /// <param name="functionName">Function name to search for.</param>
        /// <param name="argumentTypes">Argument types for the function.</param>
        public static MethodInfo GetValidMethodInfo(Type objectType, string functionName, Type[] argumentTypes)
        {
            while (objectType != typeof(object) && objectType != null)
            {
                var method = objectType.GetMethod(functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, argumentTypes, null);
                if (method != null)
                {
                    var parameterInfos = method.GetParameters();
                    var methodValid = true;
                    var i = 0;
                    foreach (ParameterInfo pi in parameterInfos)
                    {
                        var requestedType = argumentTypes[i];
                        var receivedType = pi.ParameterType;
                        methodValid = requestedType.IsPrimitive == receivedType.IsPrimitive;

                        if (!methodValid)
                            break;
                        i++;
                    }
                    if (methodValid)
                        return method;
                }
                objectType = objectType.BaseType;
            }
            return null;
        }

        protected bool ValidateRegistration(MethodInfo method, object targetObj, ListenerMode mode)
        {
            return ValidateRegistration(method, targetObj, mode, typeof(Object));
        }

        protected bool ValidateRegistration(MethodInfo method, object targetObj, ListenerMode mode, Type argumentType)
        {
            if (method == null)
                throw new System.ArgumentNullException("method", string.Format("Can not register null method on {0} for callback!", targetObj));
            Object @object = targetObj as Object;
            if (@object == null || @object.GetInstanceID() == 0)
                throw new System.ArgumentException(string.Format("Could not register callback {0} on {1}. The class {2} does not derive from UnityEngine.Object", method.Name, targetObj, (targetObj != null ? targetObj.GetType().ToString() : "null")));
            if (method.IsStatic)
                throw new System.ArgumentException(string.Format("Could not register listener {0} on {1} static functions are not supported.", method, GetType()));
            if (FindMethod(targetObj, method.Name, mode, argumentType) != null)
                return true;
            Debug.LogWarning(string.Format("Could not register listener {0}.{1} on {2} the method could not be found.", targetObj, method, GetType()));
            return false;
        }

        protected bool ValidateRegistration(MethodInfo method, object targetObj, PersistentListenerMode mode, Type argumentType)
        {
            if (method == null)
                throw new System.ArgumentNullException("method", string.Format("Can not register null method on {0} for callback!", targetObj));

            if (method.DeclaringType == null)
            {
                throw new System.NullReferenceException(
                    string.Format(
                        "Method '{0}' declaring type is null, global methods are not supported",
                        method.Name));
            }

            Type targetType;
            if (!method.IsStatic)
            {
                var obj = targetObj as Object;
                if (obj == null || obj.GetInstanceID() == 0)
                {
                    throw new System.ArgumentException(
                        string.Format(
                            "Could not register callback {0} on {1}. The class {2} does not derive from UnityEngine.Object",
                            method.Name,
                            targetObj,
                            targetObj == null ? "null" : targetObj.GetType().ToString()));
                }

                targetType = obj.GetType();

                if (!method.DeclaringType.IsAssignableFrom(targetType))
                    throw new System.ArgumentException(
                        string.Format(
                            "Method '{0}' declaring type '{1}' is not assignable from object type '{2}'",
                            method.Name,
                            method.DeclaringType.Name,
                            obj.GetType().Name));
            }
            else
            {
                targetType = method.DeclaringType;
            }

            if (FindMethod(method.Name, targetType, mode, argumentType) == null)
            {
                Debug.LogWarning(string.Format("Could not register listener {0}.{1} on {2} the method could not be found.", targetObj, method, GetType()));
                return false;
            }
            return true;
        }

        internal void AddPersistentListener()
        {
            m_PersistentCalls.AddListener();
        }

        protected void RegisterPersistentListener(int index, object targetObj, MethodInfo method)
        {
            RegisterPersistentListener(index, targetObj, targetObj.GetType(), method);
        }

        protected void RegisterPersistentListener(int index, object targetObj, Type targetObjType, MethodInfo method)
        {
            if (!ValidateRegistration(method, targetObj, ListenerMode.EventDefined))
                return;

            m_PersistentCalls.RegisterEventPersistentListener(index, targetObj as Object, targetObjType, method.Name);
            DirtyPersistentCalls();
        }

        internal void RemovePersistentListener(Object target, MethodInfo method)
        {
            if (method == null || method.IsStatic || (target == null || target.GetInstanceID() == 0))
                return;
            m_PersistentCalls.RemoveListeners(target, method.Name);
            DirtyPersistentCalls();
        }

        internal void RemovePersistentListener(int index)
        {
            m_PersistentCalls.RemoveListener(index);
            DirtyPersistentCalls();
        }

        internal void UnregisterPersistentListener(int index)
        {
            m_PersistentCalls.UnregisterPersistentListener(index);
            DirtyPersistentCalls();
        }

        internal void AddVoidPersistentListener(UnityAction call)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVoidPersistentListener(persistentEventCount, call);
        }

        internal void RegisterVoidPersistentListener(int index, UnityAction call)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
                return;
            }
            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Void))
                return;

            m_PersistentCalls.RegisterVoidPersistentListener(index, call.Target as Object, call.Method.DeclaringType, call.Method.Name);
            DirtyPersistentCalls();
        }

        internal void AddIntPersistentListener(UnityAction<int> call, int argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterIntPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterIntPersistentListener(int index, UnityAction<int> call, int argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
                return;
            }
            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Int))
                return;

            m_PersistentCalls.RegisterIntPersistentListener(index, call.Target as Object, call.Method.DeclaringType, argument, call.Method.Name);
            DirtyPersistentCalls();
        }

        internal void AddFloatPersistentListener(UnityAction<float> call, float argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterFloatPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterFloatPersistentListener(int index, UnityAction<float> call, float argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
                return;
            }
            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Float))
                return;

            m_PersistentCalls.RegisterFloatPersistentListener(index, call.Target as Object, call.Method.DeclaringType, argument, call.Method.Name);
            DirtyPersistentCalls();
        }

        internal void AddBoolPersistentListener(UnityAction<bool> call, bool argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterBoolPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterBoolPersistentListener(int index, UnityAction<bool> call, bool argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
                return;
            }
            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Bool))
                return;

            m_PersistentCalls.RegisterBoolPersistentListener(index, call.Target as Object, call.Method.DeclaringType, argument, call.Method.Name);
            DirtyPersistentCalls();
        }

        internal void AddEnumPersistentListener(UnityAction<Enum> call, Enum argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterEnumPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterEnumPersistentListener(int index, UnityAction<Enum> call, Enum argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Bool))
                    return;
                m_PersistentCalls.RegisterEnumPersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }

        internal void AddStringPersistentListener(UnityAction<string> call, string argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterStringPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterStringPersistentListener(int index, UnityAction<string> call, string argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
                return;
            }
            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.String))
                return;

            m_PersistentCalls.RegisterStringPersistentListener(index, call.Target as Object, call.Method.DeclaringType, argument, call.Method.Name);
            DirtyPersistentCalls();
        }


        internal void AddObjectPersistentListener<T>(UnityAction<T> call, T argument) where T : Object
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterObjectPersistentListener<T>(persistentEventCount, call, argument);
        }

        internal void RegisterObjectPersistentListener<T>(int index, UnityAction<T> call, T argument) where T : Object
        {
            if (call == null)
                throw new System.ArgumentNullException("call", "Registering a Listener requires a non null call");

            if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Object, argument == null ? typeof(Object) : argument.GetType()))
                return;

            m_PersistentCalls.RegisterObjectPersistentListener(index, call.Target as Object, call.Method.DeclaringType, argument, call.Method.Name);
            DirtyPersistentCalls();
        }

        internal void AddVector2PersistentListener(UnityAction<Vector2> call, Vector2 argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVector2PersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterVector2PersistentListener(int index, UnityAction<Vector2> call, Vector2 argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Vector2))
                    return;
                m_PersistentCalls.RegisterVector2PersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }

        internal void AddVector3PersistentListener(UnityAction<Vector3> call, Vector3 argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVector3PersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterVector3PersistentListener(int index, UnityAction<Vector3> call, Vector3 argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Bool))
                    return;
                m_PersistentCalls.RegisterVector3PersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }

#if UNITY_2017_2_OR_NEWER
        internal void AddVector2IntPersistentListener(UnityAction<Vector2Int> call, Vector2Int argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVector2IntPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterVector2IntPersistentListener(int index, UnityAction<Vector2Int> call, Vector2Int argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Vector2Int))
                    return;
                m_PersistentCalls.RegisterVector2PersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }

        internal void AddVector3IntPersistentListener(UnityAction<Vector3Int> call, Vector3Int argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVector3IntPersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterVector3IntPersistentListener(int index, UnityAction<Vector3Int> call, Vector3Int argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Bool))
                    return;
                m_PersistentCalls.RegisterVector3IntPersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }
#endif

        internal void AddVector4PersistentListener(UnityAction<Vector4> call, Vector4 argument)
        {
            int persistentEventCount = GetPersistentEventCount();
            AddPersistentListener();
            RegisterVector4PersistentListener(persistentEventCount, call, argument);
        }

        internal void RegisterVector4PersistentListener(int index, UnityAction<Vector4> call, Vector4 argument)
        {
            if (call == null)
            {
                Debug.LogWarning("Registering a Listener requires an action");
            }
            else
            {
                if (!ValidateRegistration(call.Method, call.Target, ListenerMode.Bool))
                    return;
                m_PersistentCalls.RegisterVector4PersistentListener(index, call.Target as Object, argument, call.Method.Name);
                DirtyPersistentCalls();
            }
        }

        private InvokableCallList Calls
        {
            get
            {
                if (m_Calls == null)
                    m_Calls = new InvokableCallList();

                return m_Calls;
            }
        }
    }


    public enum ListenerMode
    {
        /// <summary>
        /// The listener will use the function binding specified by the event.
        /// </summary>
        EventDefined,
        /// <summary>
        /// The listener will bind to zero argument functions.
        /// </summary>
        Void,
        /// <summary>
        /// The listener will bind to an Object type argument functions.
        /// </summary>
        Object,
        /// <summary>
        /// The listener will bind to an int argument functions.
        /// </summary>
        Int,
        /// <summary>
        /// The listener will bind to a float argument functions.
        /// </summary>
        Float,
        /// <summary>
        /// The listener will bind to a string argument functions.
        /// </summary>
        String,
        /// <summary>
        /// The listener will bind to a bool argument functions.
        /// </summary>
        Bool,
        /// <summary>
        /// The listener will bind to an enum argument functions.
        /// </summary>
        Enum,
        /// <summary>
        /// The listener will bind to a Vector2 argument functions.
        /// </summary>
        Vector2,
        /// <summary>
        /// The listener will bind to a Vector2Int argument functions.
        /// </summary>
        Vector2Int,
        /// <summary>
        /// The listener will bind to a Vector3 argument functions.
        /// </summary>
        Vector3,
        /// <summary>
        /// The listener will bind to a Vector3Int argument functions.
        /// </summary>
        Vector3Int,
        /// <summary>
        /// The listener will bind to a Vector4 argument functions.
        /// </summary>
        Vector4,
        /// <summary>
        /// The listener will bind to a LayerMask argument functions.
        /// </summary>
        LayerMask,
        /// <summary>
        /// The listener will bind to a Color argument functions.
        /// </summary>
        Color,
        /// <summary>
        /// The listener will bind to a Quaternion argument functions.
        /// </summary>
        Quaternion
    }
}