using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace Test_FMLib
{
  [TestClass]
  public class TEST_Doc
  {
    [TestMethod]
    public void TestMethod1()
    {
      var doc = new Doc("name", "desc")
      {
        Default = "default",
        Examples = new List<string>() { "ex1", "ex2" },
        PossibleValues = "possible values",
        Remark = "remark"
      };
      var s = doc.Get().ToString();
    }
  }
}
