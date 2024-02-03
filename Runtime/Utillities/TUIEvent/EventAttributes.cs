namespace TUI.Utillities
{
	using System;
	using UnityEngine;
	public class EventAttributes : MonoBehaviour
	{
		/// <summary>
		/// Use this attribute to turn an integer parameter into a layer enum.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
		public class LayerAttribute : PropertyAttribute
		{
			/// <summary>
			/// The default value to display.
			/// </summary>
			public readonly int defaultValue = 0;

			/// <param name="layerIndex">The layer index</param>
			public LayerAttribute(int layerIndex = 0)
			{
				if (string.IsNullOrEmpty(LayerMask.LayerToName(layerIndex)))
					layerIndex = 0;

				defaultValue = layerIndex;
			}

			/// <param name="layerName">The layer name</param>
			public LayerAttribute(string layerName)
			{
				int layer = LayerMask.NameToLayer(layerName);
				defaultValue = layer > -1 ? layer : 0;
			}
		}

		/// <summary>
		/// Use this attribute to turn a float parameter into a slider
		/// with min and max values.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
		public class SliderAttribute : PropertyAttribute
		{
			/// <summary>
			/// The min value.
			/// </summary>
			public readonly float minValue = 0f;

			/// <summary>
			/// The max value.
			/// </summary>
			public readonly float maxValue = 0f;

			/// <param name="min">The min value</param>
			/// <param name="max">The max value</param>
			public SliderAttribute(float min, float max)
			{
				if (max < min)
					max = min;

				minValue = min;
				maxValue = max;
			}
		}

		/// <summary>
		/// Use this attribute to turn an integer parameter into a slider
		/// with min and max values.
		/// </summary>
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
		public class IntSliderAttribute : PropertyAttribute
		{
			/// <summary>
			/// The min value.
			/// </summary>
			public readonly int minValue = 0;

			/// <summary>
			/// The max value.
			/// </summary>
			public readonly int maxValue = 0;

			/// <param name="min">The min value</param>
			/// <param name="max">The max value</param>
			public IntSliderAttribute(int min, int max)
			{
				if (max < min)
					max = min;

				minValue = min;
				maxValue = max;
			}
		}

		/// <summary>
		/// Use this attribute to trigger a custom method to build the arguments on the inspector.
		/// Note that the method specified by "methodName" must have signature like below:
		/// <code>
		/// For methods:
		/// public void ExampleMethod(SerializedProperty arguments, Rect argNameRect, Rect argRect) { }
		/// </code>
		/// <code>
		/// For parameters:
		/// public void ExampleMethod(SerializedProperty argument, Rect argNameRect, Rect argRect) { }
		/// </code>
		/// </summary>
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
		public class CustomInspectorAttribute : PropertyAttribute
		{
			public readonly string methodName = null;
			public CustomInspectorAttribute(string methodName) => this.methodName = methodName;
		}


	}
}
