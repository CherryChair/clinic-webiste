import { useEffect, useState } from "react";
import EmployeeListElement from "../components/EmployeeListElement";
import axios from "axios";
import ErrorBox from "../components/ErrorBox";
import { isAdmin } from "./Login"


export default function EmployeeListPage() {
    
    const [employees, setEmployees] = useState([]);
    const [specialities, setSpecialities] = useState([]);
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");

    useEffect(() => {
        getEmployees();
        getSpecialities();
    }, []);
    
    
    const getEmployees = () => {
        axios.get("/Employees/list").then(response => {
        setEmployees(response.data);
        }).catch(error => console.log(error));
    };

    const getSpecialities = () => {
        axios.get("/Specialities/list").then(response => {
        setSpecialities(response.data);
        }).catch(error => console.log(error));
    };

    const setError = (msg) => {
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };

    const handleDelete = () => {
        window.location.reload();
    };
  
    return (
        <div className="relative overflow-x-auto shadow-md sm:rounded-lg w-9/12 m-auto mt-16">
            <ErrorBox changeErrorFlag={clearErrorFlag} errorFlag={errorFlag} errorMsg={errorMsg}/>
            <table className="w-full text-sm text-left rtl:text-right text-gray-500 ">
                <thead className="text-xs text-gray-700 uppercase bg-gray-50  ">
                    <tr>
                        <th scope="col" className="px-6 py-3">
                            Name
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Email
                        </th>
                        <th scope="col" className="px-6 py-3">
                            Speciality
                        </th>
                        <th scope="col" className="px-6 py-3">
                            <span className="sr-only">Edit</span>
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {employees.map(item => (<EmployeeListElement 
                        id={item.id} 
                        firstName={item.firstName} 
                        surname={item.surname} 
                        email={item.email} 
                        speciality={item.specialityId ? specialities[item.specialityId] : ""}
                        concurrencyStamp={item.concurrencyStamp} 
                        setErrorFunc={setError}
                        onDelete={handleDelete}/>))
                    }
                </tbody>
            </table>
        </div>
    );
}