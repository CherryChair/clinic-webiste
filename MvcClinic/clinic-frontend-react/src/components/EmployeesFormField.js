import axios from "axios";
import { useEffect, useState } from "react";

export default function EmployeesFormField({className, defaultValue}) {
    let css = "block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6";
    if (className) {
        css += className;
    }

    const [employee, setEmployees] = useState([]);

    useEffect(() => {
      getEmployees();
    }, []);
    

    const getEmployees = () => {
      axios.get("/Employees/idList").then(response => {
      setEmployees(response.data);
      }).catch(error => console.log(error));
    }

    return (
      <div>
        <div className="flex items-center justify-between">
          <label htmlFor="employeeId" className="block text-sm font-medium leading-6 text-gray-900">
            Employee
          </label>
        </div>
        <div className="mt-2">
          <select id="employeeId" className={css}>
            <option value="" key=""></option>
            {employee.map((item) => (
              <option value={item.id} key={item.id} selected={item.id===defaultValue}>{item.name}</option>
              ))}
          </select>
        </div>
      </div>
    );
}