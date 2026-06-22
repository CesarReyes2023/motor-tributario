using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LibroFiscal.Persistence;
var options = new DbContextOptionsBuilder<LibroFiscalDbContext>().UseNpgsql("Host=localhost;Database=librofiscal;Username=postgres;Password=postgres").Options;
using var ctx = new LibroFiscalDbContext(options);
Console.WriteLine("DTEs count: " + ctx.Dtes.Count());
var maxDate = ctx.Dtes.Max(d => (DateTime?)d.FechaEmision);
Console.WriteLine("Max DTE Date: " + maxDate);
