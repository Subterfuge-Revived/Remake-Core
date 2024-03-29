﻿using NUnit.Framework;
using Subterfuge.Remake.Server.Test.util;

namespace Subterfuge.Remake.Server.Test.test.admin;

public class AdminControllerTest
{

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }
    
    [Test]
    public async Task AdministratorCanBanAPlayer(){}
    
    [Test]
    public async Task AdministratorCanViewAPlayersBanHistory(){}
    
    [Test]
    public async Task AdministratorCanBanAnIp(){}
    
    [Test]
    public async Task AdministratorCanBanAnIpByRegex(){}
    
    [Test]
    public async Task AdministratorCanViewTheServerExceptionLog(){}
    
    [Test]
    public async Task AdministratorCanSearchTheServerExceptionLogCaseInsensitive(){}
    
    [Test]
    public async Task AdministratorCanViewAListOfPlayerActions(){}
    
    [Test]
    public async Task AdministratorCanSearchTheListOfPlayerActionsCaseInsensitive(){}
}