using System;
using System.Collections.Generic;
using FMLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_FMLib
{
  [TestClass]
  public class Test_PropertyAutomation
  {
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void Test_ToString_BasicTypes()
    {
      var data = new List<object> { 1, 4.5, "qqq", 'w', true, };
      var result = new List<string>() { };
      var check = new List<string> { "1", "4.5", "qqq", "w", "True" };
      foreach (var e in data) { result.Add(PropertyAutomation.ToString(e)); }
      string error = "";
      for(int i=0; i<result.Count;++i)
      {
        if(result[i]!= check[i]) { error += $"\nexpected:{check[i]}, got:{result[i]}"; }
      }
      if(result.Count!= check.Count) { error += "\n Resulting amount of values is different from expected"; }
      Assert.AreEqual("", error);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void Test_ToString_List()
    {
      var data = new List<object> { 1, 4.5, "qqq", 'w', true, };
      var result = PropertyAutomation.ToString(data);
      var check = "{1, 4.5, qqq, w, True}";
      Assert.AreEqual(check, result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void Test_ToString_Dictionary()
    {
      var data = new Dictionary<object, object> { { 1, "q" }, { 'c', null } };
      var result = PropertyAutomation.ToString(data);
      var check = "{\"1\":\"q\", \"c\":\"{NULL}\"}";
      Assert.AreEqual(check, result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void Test_ToString_UserClass()
    {
      object data = new UserClassNoProps();
      var result = PropertyAutomation.ToString(data);
      var check = "{}";
      Assert.AreEqual(check, result);

      data = new UserClass1();
      result = PropertyAutomation.ToString(data);
      check = "{\"MyProperty\":\"999\"}";
      Assert.AreEqual(check, result);

      data = new UserClass2();
      result = PropertyAutomation.ToString(data);
      check = "{\"MyProperty\":\"888\", \"YourProperty\":\"xaxa\"}";
      Assert.AreEqual(check, result);

      data = new UserClass3();
      result = PropertyAutomation.ToString(data);
      check = "{\"MyProperty\":\"888\", \"YourProperty\":\"xaxa\", \"DictProp\":\"{NULL}\"}";
      Assert.AreEqual(check, result);

      data = new UserClass3() { DictProp = new Dictionary<string, Test_PropertyAutomation.UserClass1> { { "key1", new UserClass1() } } };
      result = PropertyAutomation.ToString(data);
      check = "{\"MyProperty\":\"888\", \"YourProperty\":\"xaxa\", \"DictProp\":\"{\"key1\":\"{\"MyProperty\":\"999\"}\"}\"}";
      Assert.AreEqual(check, result);
    }





























    private class UserClassNoProps
    {

    }

    private class UserClass1
    {
      public int MyProperty { get; set; } = 999;
    }

    private class UserClass2
    {
      public int MyProperty { get; set; } = 888;
      public string YourProperty { get; set; } = "xaxa";
    }

    private class UserClass3
    {
      public int MyProperty { get; set; } = 888;
      public string YourProperty { get; set; } = "xaxa";
      public Dictionary<string, UserClass1> DictProp { get; set; }
    }
  }
}
