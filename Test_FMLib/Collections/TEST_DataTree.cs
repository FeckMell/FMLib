using System;
using System.Diagnostics;
using FMLib.Collections;
using FMLib.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_FMLib.Collections
{
  [TestClass]
  public class TEST_DataTree
  {

    [TestMethod]
    public void TestConstructors()
    {
      string nullString = null;
      DataTree<string> nullDataTree = null;

      //default
      var a_val_default = new DataTree<int>();
      var a_ref_default = new DataTree<string>();

      //.(T value) - value
      var b_val_value = new DataTree<int>(1);
      var b_ref_value = new DataTree<string>("1");
      var b_ref_value1 = new DataTree<string>(nullString);
      var b_ref_value2 = new DataTree<string>(string.Empty);

      //.(DataTree<T> node) - node
      var c_val_value = new DataTree<int>(b_val_value);
      var c_ref_value = new DataTree<string>(b_ref_value);
      var c_ref_value1 = new DataTree<string>(b_ref_value1);
      var c_ref_value2 = new DataTree<string>(b_ref_value2);
      var c_ref_value3 = new DataTree<string>(nullDataTree);

      //.(T value, DataTree<T> node) - value_node
      var d_ref_value = new DataTree<string>("3", b_ref_value);
      var d_ref_value1 = new DataTree<string>(nullString, b_ref_value);
      var d_ref_value2 = new DataTree<string>(nullString, nullDataTree);
      var d_ref_value3 = new DataTree<string>("3", nullDataTree);
      var d_ref_value4 = new DataTree<string>("3.1", d_ref_value);

      d_ref_value4.Add(d_ref_value);
      d_ref_value4.Add(d_ref_value);
      d_ref_value4.Add(d_ref_value4);// todo: loop handling

      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      string s;
      for (int i = 0; i < 100; i++)
      {
        s = d_ref_value4.ToString("\n", "+");
        //s = d_ref_value4.AsString();
      }
      stopwatch.Stop();
      var elapsed = stopwatch.ElapsedMilliseconds;
      Console.WriteLine(elapsed);
      string ss = d_ref_value4.AsString();
    }
  }
}
