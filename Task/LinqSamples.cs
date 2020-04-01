// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Where - Task 1")]
        [Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
        public void LinqSample1()
        {
            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

            var lowNums =
                from num in numbers
                where num < 5
                select num;

            Console.WriteLine("Numbers < 5:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 2")]
        [Description("This sample return return all presented in market products")]

        public void LinqSample2()
        {
            var products =
                from p in dataSource.Products
                where p.UnitsInStock > 0
                select p;

            foreach (var p in products)
            {
                ObjectDumper.Write(p);
            }
        }

        [Description(@"1.Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) 
        превосходит некоторую величину X. Продемонстрируйте выполнение запроса с различными X 
        (подумайте, можно ли обойтись без копирования запроса несколько раз)")]
        public void Linq1()
        {
            decimal x = 1500;
            var customers = dataSource.Customers.
                Where(c => c.Orders.Sum(o => o.Total) > x).
                Select(c => new
                {
                    CustomerId = c.CustomerID,
                    Total = c.Orders.Sum(o => o.Total)
                });

            ObjectDumper.Write($"Total sum greater than {x}");
            foreach (var p in customers)
            {
                ObjectDumper.Write(p);
            }

            x = 12000.16m;
            ObjectDumper.Write($"Total sum greater than {x}");
            foreach (var p in customers)
            {
                ObjectDumper.Write(p);
            }
        }

        [Description(@"2.Для каждого клиента составьте список поставщиков, находящихся в той же стране 
и том же городе. Сделайте задания с использованием группировки и без.")]
        public void Linq2()
        {
            var customersSuppliers = from cust in dataSource.Customers
                                     join sup in dataSource.Suppliers on new { cust.Country, cust.City } equals new { sup.Country, sup.City }
                                     orderby cust.Country
                                     select new
                                     {
                                         Country = cust.Country,
                                         City = cust.City,
                                         Customer = cust.CustomerID,
                                         Suplier = sup.SupplierName
                                     };

            foreach (var cs in customersSuppliers)
            {
                ObjectDumper.Write(cs);
            }

            var customersSuppliers2 = from cust in dataSource.Customers
                                      join sup in dataSource.Suppliers
                                          on new { cust.Country, cust.City } equals new { sup.Country, sup.City } into cs
                                      from c in cs.DefaultIfEmpty()
                                      orderby cust.Country
                                      select new
                                      {
                                          Country = cust.Country,
                                          City = cust.City,
                                          Customer = cust.CustomerID,
                                          Supplier = c != null ? c.SupplierName : ""
                                      };

            foreach (var cs in customersSuppliers2)
            {
                ObjectDumper.Write(cs);
            }
        }

        [Description(@"Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
        public void Linq3()
        {
            decimal x = 1000m;
            var customers = dataSource.Customers.
                Where(c => c.Orders.Any(order => order.Total > x)).
                Select(c => new
                {
                    CustomerId = c.CustomerID,
                    OrderTotal = c.Orders.Where(order => order.Total > x).Select(o => o.Total)
                });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer.CustomerId);
                ObjectDumper.Write(customer.OrderTotal);
            }
        }

        [Description(@"4.Выдайте список клиентов с указанием, начиная с какого месяца какого года они
стали клиентами(принять за таковые месяц и год самого первого заказа)")]
        public void Linq4()
        {
            var customers = dataSource.Customers.
                Select(c => new
                {
                    Value = c,
                    Year = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.Year,
                    Month = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.Month
                });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Description(@"5.Сделайте предыдущее задание, но выдайте список отсортированным по 
году, месяцу, оборотам клиента(от максимального к минимальному) и имени клиента")]
        public void Linq5()
        {
            var customers = dataSource.Customers
                .Select(c => new
                {
                    Value = c,
                    Year = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.Year,
                    Month = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate.Month
                })
                .OrderBy(c => c.Year)
                .ThenBy(c => c.Month)
                .ThenByDescending(c => c.Value.Orders.Sum(o => o.Total))
                .ThenBy(c => c.Value.CustomerID);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Description(@"6.Укажите всех клиентов, у которых указан нецифровой почтовый код 
или не заполнен регион или в телефоне не указан код оператора
(для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
        public void Linq6()
        {
            var customers = dataSource.Customers
                .Where(c => (string.IsNullOrEmpty(c.PostalCode) || !c.PostalCode.All(char.IsDigit)
                                                                || string.IsNullOrEmpty(c.Region)
                                                                || !c.Phone.StartsWith("(")));

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Description(@"7.Сгруппируйте все продукты по категориям, внутри – по наличию на складе,
внутри последней группы отсортируйте по стоимости")]
        public void Linq7()
        {
            var products = dataSource.Products
                    .GroupBy(p => p.Category)
                    .Select(p => new
                    {
                        Category = p.Key,
                        UnitsInStock = p.GroupBy(u => u.UnitsInStock)
                            .Select(a => new
                            {
                                QuntityInStock = a.Key,
                                Products = a.OrderBy(pr => pr.UnitPrice)
                            })
                    });
               
            foreach (var product in products)
            {
                ObjectDumper.Write(product);
            }
        }

        [Description(@"8.Cгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». 
Границы каждой группы задайте сами")]
        public void Linq8()
        {
            var averageBottomBoundary = 50.00m;
            var averageTopBoundary = 200.00m;


            var products = dataSource.Products
                .GroupBy(p => p.UnitPrice <= averageBottomBoundary ? "Low" : 
                    p.UnitPrice >averageBottomBoundary && p.UnitPrice<=averageTopBoundary ? "Average" : 
                    "Expensive")
                .Select(p => new
                {
                    Category = p.Key,
                    Price = p.GroupBy(pp => pp.UnitPrice).Select(ppp=>ppp.Key)
                });

            foreach (var product in products)
            {
                ObjectDumper.Write(product.Category);
                ObjectDumper.Write(product.Price);
            }
        }
    }
    //9.	Рассчитайте среднюю прибыльность каждого города(среднюю сумму заказа по всем клиентам из данного города) и среднюю интенсивность(среднее количество заказов, приходящееся на клиента из каждого города)
    //10.	Сделайте среднегодовую статистику активности клиентов по месяцам(без учета года), статистику по годам, по годам и месяцам(т.е.когда один месяц в разные годы имеет своё значение).



}
