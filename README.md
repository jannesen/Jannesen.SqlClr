# Jannesen.SqlClr

This library implements the following datatype for SQL Server

- idata - a date stored in 2 bytes. The epoch is 1970-01-01 so it can store a date between 1900-01-01 and 2059-01-01
- idaterange - a idate range (4 bytes).
- idatetimerange - a daterange (16 bytes). 

