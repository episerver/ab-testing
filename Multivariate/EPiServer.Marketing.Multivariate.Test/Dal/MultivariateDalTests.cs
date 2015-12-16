//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System.Data;
//using EPiServer.Marketing.Multivariate.Dal;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Data.Common;

//namespace EPiServer.Marketing.Multivariate.Test.Dal
//{
//    [TestClass]
//    public class MultivariateDalTests
//    {
//        private string connectionString = "ConnectionString";
//        private Mock<IDataOperations> dataOperationsMock;

//        private MultivariateTestDal GetUnitUnderTest()
//        {
//            dataOperationsMock = new Mock<IDataOperations>();
//            return new MultivariateTestDal(connectionString, dataOperationsMock.Object);
//        }

//        [TestMethod]
//        public void Add_Calls_ExecuteNonQuery_WithCorrectParameters()
//        {
//            var testDal = GetUnitUnderTest();
//            MultivariateTestParameters parameters = new MultivariateTestParameters()
//            {
//                Title = "Multivariate Test 1",
//                Owner = "John Doe",
//                OriginalItemId = new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4"),
//                VariantItemId = new Guid("E17251CD-EE97-4001-8E0B-3A38C80523F3"),
//                ConversionItemId = new Guid("8286E387-5000-4653-A107-6E6A3F844C1F"),
//                State = "Active",
//                StartDate = new DateTime(2015, 11, 1),
//                EndDate = new DateTime(2015, 11, 30),
//                LastModifiedDate = new DateTime(2015, 6, 22),
//                LastModifiedBy = "Jane Doe"
//            };

//            dataOperationsMock.Setup(x => x.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter[]>()));
//            var newGuid = testDal.Add(parameters);
//            dataOperationsMock.Verify(d => d.ExecuteNonQuery(It.Is<string>(arg => arg == "MultivariateTest_Save"),
//                                                             It.Is<CommandType>(arg => arg == CommandType.StoredProcedure),
//                                                             It.Is<SqlParameter[]>(arg => VerifyParametersForAdd(arg))));
//        }

//        private bool VerifyParametersForAdd(SqlParameter[] args)
//        {
//            return (args.Length == 11) &&
//                   (args[0].ParameterName == "Id" && (Guid)args[0].Value != Guid.Empty && args[0].DbType == DbType.Guid && args[0].Size == 36) &&
//                   (args[1].ParameterName == "Title" && (string)args[1].Value == "Multivariate Test 1" && args[1].DbType == DbType.String && args[1].Size == 255) &&
//                   (args[2].ParameterName == "Owner" && (string)args[2].Value == "John Doe" && args[2].DbType == DbType.String && args[2].Size == 100) &&
//                   (args[3].ParameterName == "OriginalItemId" && (Guid)args[3].Value == new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4") && args[3].DbType == DbType.Guid && args[3].Size == 36) &&
//                   (args[4].ParameterName == "VariantItemId" && (Guid)args[4].Value == new Guid("E17251CD-EE97-4001-8E0B-3A38C80523F3") && args[4].DbType == DbType.Guid && args[4].Size == 36) &&
//                   (args[5].ParameterName == "ConversionItemId" && (Guid)args[5].Value == new Guid("8286E387-5000-4653-A107-6E6A3F844C1F") && args[5].DbType == DbType.Guid && args[5].Size == 36) &&
//                   (args[6].ParameterName == "State" && (string)args[6].Value == "Active" && args[6].DbType == DbType.String && args[6].Size == 10) &&
//                   (args[7].ParameterName == "StartDate" && (DateTime)args[7].Value == new DateTime(2015, 11, 1) && args[7].DbType == DbType.DateTime && args[7].Size == 12) &&
//                   (args[8].ParameterName == "EndDate" && (DateTime)args[8].Value == new DateTime(2015, 11, 30) && args[8].DbType == DbType.DateTime && args[8].Size == 12) &&
//                   (args[9].ParameterName == "LastModifiedDate" && (DateTime)args[9].Value == new DateTime(2015, 6, 22) && args[9].DbType == DbType.DateTime && args[9].Size == 12) &&
//                   (args[10].ParameterName == "LastModifiedBy" && (string)args[10].Value == "Jane Doe" && args[10].DbType == DbType.String && args[10].Size == 100);
//        }

//        [TestMethod]
//        public void Update_Calls_ExecuteNonQuery_WithCorrectParameters()
//        {
//            var testDal = GetUnitUnderTest();
//            MultivariateTestParameters parameters = new MultivariateTestParameters()
//            {
//                Id = new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585"),
//                Title = "Multivariate Test 1",
//                Owner = "John Doe",
//                OriginalItemId = new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4"),
//                VariantItemId = new Guid("E17251CD-EE97-4001-8E0B-3A38C80523F3"),
//                ConversionItemId = new Guid("8286E387-5000-4653-A107-6E6A3F844C1F"),
//                State = "Active",
//                StartDate = new DateTime(2015, 11, 1),
//                EndDate = new DateTime(2015, 11, 30),
//                LastModifiedDate = new DateTime(2015, 6, 22),
//                LastModifiedBy = "Jane Doe"
//            };

//            dataOperationsMock.Setup(x => x.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter[]>()));
//            testDal.Update(parameters);
//            dataOperationsMock.Verify(d => d.ExecuteNonQuery(It.Is<string>(arg => arg == "MultivariateTest_Save"),
//                                                             It.Is<CommandType>(arg => arg == CommandType.StoredProcedure),
//                                                             It.Is<SqlParameter[]>(arg => VerifyParametersForUpdate(arg))));
//        }

//        private bool VerifyParametersForUpdate(SqlParameter[] args)
//        {
//            return (args.Length == 11) &&
//                   (args[0].ParameterName == "Id" && (Guid)args[0].Value == new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585") && args[0].DbType == DbType.Guid && args[0].Size == 36) &&
//                   (args[1].ParameterName == "Title" && (string)args[1].Value == "Multivariate Test 1" && args[1].DbType == DbType.String && args[1].Size == 255) &&
//                   (args[2].ParameterName == "Owner" && (string)args[2].Value == "John Doe" && args[2].DbType == DbType.String && args[2].Size == 100) &&
//                   (args[3].ParameterName == "OriginalItemId" && (Guid)args[3].Value == new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4") && args[3].DbType == DbType.Guid && args[3].Size == 36) &&
//                   (args[4].ParameterName == "VariantItemId" && (Guid)args[4].Value == new Guid("E17251CD-EE97-4001-8E0B-3A38C80523F3") && args[4].DbType == DbType.Guid && args[4].Size == 36) &&
//                   (args[5].ParameterName == "ConversionItemId" && (Guid)args[5].Value == new Guid("8286E387-5000-4653-A107-6E6A3F844C1F") && args[5].DbType == DbType.Guid && args[5].Size == 36) &&
//                   (args[6].ParameterName == "State" && (string)args[6].Value == "Active" && args[6].DbType == DbType.String && args[6].Size == 10) &&
//                   (args[7].ParameterName == "StartDate" && (DateTime)args[7].Value == new DateTime(2015, 11, 1) && args[7].DbType == DbType.DateTime && args[7].Size == 12) &&
//                   (args[8].ParameterName == "EndDate" && (DateTime)args[8].Value == new DateTime(2015, 11, 30) && args[8].DbType == DbType.DateTime && args[8].Size == 12) &&
//                   (args[9].ParameterName == "LastModifiedDate" && (DateTime)args[9].Value == new DateTime(2015, 6, 22) && args[9].DbType == DbType.DateTime && args[9].Size == 12) &&
//                   (args[10].ParameterName == "LastModifiedBy" && (string)args[10].Value == "Jane Doe" && args[10].DbType == DbType.String && args[10].Size == 100);
//        }

//        [TestMethod]
//        public void Delete_Calls_ExecuteNonQuery_WithCorrectParameters()
//        {
//            var testDal = GetUnitUnderTest();
//            var Id = new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585");
//            dataOperationsMock.Setup(x => x.ExecuteNonQuery(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter[]>()));
//            testDal.Delete(Id);
//            dataOperationsMock.Verify(d => d.ExecuteNonQuery(It.Is<string>(arg => arg == "MultivariateTest_Delete"),
//                                                             It.Is<CommandType>(arg => arg == CommandType.StoredProcedure),
//                                                             It.Is<SqlParameter[]>(arg => VerifyParametersForDelete(arg))));
//        }

//        private bool VerifyParametersForDelete(SqlParameter[] args)
//        {
//            return (args.Length == 1) &&
//                   (args[0].ParameterName == "Id" && (Guid)args[0].Value == new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585") && args[0].DbType == DbType.Guid && args[0].Size == 36);
//        }

//        [TestMethod]
//        public void Get_Calls_ExecuteReader_WithCorrectParameters()
//        {
//            var testDal = GetUnitUnderTest();
//            var Id = new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585");

//            dataOperationsMock.Setup(x => x.ExecuteReader(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter[]>(), It.IsAny<Action<IDataReader>>()));

//            testDal.Get(Id);
//            dataOperationsMock.Verify(d => d.ExecuteReader(It.Is<string>(arg => arg == "MultivariateTest_GetTest"),
//                                                             It.Is<CommandType>(arg => arg == CommandType.StoredProcedure),
//                                                             It.Is<SqlParameter[]>(arg => VerifyParametersForGet(arg)),
//                                                             It.IsAny<Action<IDataReader>>()));
//        }

//        private bool VerifyParametersForGet(SqlParameter[] args)
//        {
//            return (args.Length == 1) &&
//                   (args[0].ParameterName == "Id" && (Guid)args[0].Value == new Guid("3FA43918-3CC1-452F-B1EC-C63F590BD585") && args[0].DbType == DbType.Guid && args[0].Size == 36);
//        }

//        [TestMethod]
//        public void GetByOriginalItemId_Calls_ExecuteReader_WithCorrectParameters()
//        {
//            var testDal = GetUnitUnderTest();
//            var originalItemId = new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4");
//            dataOperationsMock.Setup(x => x.ExecuteReader(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter[]>(), It.IsAny<Action<IDataReader>>()));
//            testDal.GetByOriginalItemId(originalItemId);
//            dataOperationsMock.Verify(d => d.ExecuteReader(It.Is<string>(arg => arg == "MultivariateTest_GetByOriginalItemId"),
//                                                             It.Is<CommandType>(arg => arg == CommandType.StoredProcedure),
//                                                             It.Is<SqlParameter[]>(arg => VerifyParametersForGetByOriginalItemId(arg)),
//                                                             It.IsAny<Action<IDataReader>>()));
//        }

//        private bool VerifyParametersForGetByOriginalItemId(SqlParameter[] args)
//        {
//            return (args.Length == 1) &&
//                   (args[0].ParameterName == "OriginalItemId" && (Guid)args[0].Value == new Guid("ACAEE520-FFC8-4197-AF98-B37EB77083B4") && args[0].DbType == DbType.Guid && args[0].Size == 36);
//        }
//    }
//}
