using System;
using System.Collections.Generic;
using System.Text;
using Avro;
using Avro.Specific;
	
public partial class Employee : ISpecificRecord
{
    public static Schema _SCHEMA = Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"Employee\",\"aliases\": [\"EmployeeV2\"],\"namespace\":\"TestSchema\",\"fields\":[{\"name\":\"Na" +
                                                     "me\",\"type\":\"string\"},{\"name\":\"Age\",\"type\":\"int\"}]}");
    private string _Name;
    private int _Age;
    public virtual Schema Schema
    {
        get
        {
            return Employee._SCHEMA;
        }
    }
    public string Name
    {
        get
        {
            return this._Name;
        }
        set
        {
            this._Name = value;
        }
    }
    public int Age
    {
        get
        {
            return this._Age;
        }
        set
        {
            this._Age = value;
        }
    }
    public virtual object Get(int fieldPos)
    {
        switch (fieldPos)
        {
            case 0: return this.Name;
            case 1: return this.Age;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
        };
    }
    public virtual void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0: this.Name = (System.String)fieldValue; break;
            case 1: this.Age = (System.Int32)fieldValue; break;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
        };
    }
}