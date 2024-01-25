import { useEffect, useState } from "react";
import axios from "axios";
// import { isAdmin } from "./Login"
import { useParams } from "react-router-dom";
import ErrorBox from "../components/ErrorBox";
import ButtonAccept from "../components/ButtonAccept";
import ButtonCancel from "../components/ButtonCancel";
import FormField from "../components/FormField";
import SpecialitiesFormField from "../components/SpecialitiesFormField";
import SuccessBox from "../components/SuccessBox";


export default function EmployeeEditPage() {
    const params = useParams();
    // let admin = isAdmin();
    const [employee, setEmployee] = useState(Object());
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    const [successFlag, setSuccessFlag] = useState(false);
    const [successMsg, setSuccessMsg] = useState("");

    useEffect(() => {
        axios.get(`/Employees/?id=${params.id}`).then(response => {
            setEmployee(response.data);
        }).catch(error => console.log(error));
    }, [params]);

    const setError = (msg) => {
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };

    function setSuccess(msg) {
        setSuccessFlag(true);
        setSuccessMsg(msg);
    };

    function clearSuccessFlag() {
        setSuccessFlag(false);
        setSuccessMsg("");
    }
    
    const handleSubmit = event => {
        clearErrorFlag();
        clearSuccessFlag();
        event.preventDefault();
        let formData = event.target;
        let employeePayload = {
            id: employee.id,
            firstName: formData.firstName.value,
            surname: formData.surname.value,
            email: formData.email.value,
            specialityId: formData.specialityId.value === "" ? -1 : formData.specialityId.value,
            concurrencyStamp: employee.concurrencyStamp,
        }
        axios.post("/Employees/edit", employeePayload).then(response => {
            let tempEmp = employee;
            tempEmp.concurrencyStamp = response.data;
            setEmployee(tempEmp);
            clearErrorFlag();
            setSuccess("Employee updated");
        }).catch(err => {
            console.log(err);
            clearSuccessFlag();
            if (err.response.status === 409) {
                setError("Employee data was changed");
            } else {
                setError("Not found");
            }
        });
    };

    const handleDelete = () => {
        let employeePayload = {
            id: employee.id,
            concurrencyStamp: employee.concurrencyStamp,
        }

        axios.post("/Employees/delete", employeePayload).then(response => {
            window.location.href = "/employees";
        }).catch(err => {
            console.log(err);
            if (err.response.status === 409) {
                setError("Employee data was changed");
            } else {
                setError("Not found");
            }
        });
    }

    return (
        <>
        <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
          <div className="sm:mx-auto sm:w-full sm:max-w-sm">
            <h2 className="mt-10 text-left text-2xl font-bold leading-9 tracking-tight text-gray-900">
               Edit employee
            </h2>
          </div>
  
          <div className="mt-5 sm:mx-auto sm:w-full sm:max-w-sm">
            <SuccessBox successFlag={successFlag} changeSuccessFlag={clearSuccessFlag} successMsg={successMsg}/>
            <ErrorBox errorFlag={errorFlag} changeErrorFlag={clearErrorFlag} errorMsg={errorMsg}/>
            <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="text" attr="firstName" label="First name" defaultValue={employee.firstName}/>
                <FormField type="text" attr="surname" label="Surname" defaultValue={employee.surname}/>
                <FormField type="email" attr="email" label="Email address" defaultValue={employee.email}/>
                <SpecialitiesFormField defaultValue={employee.specialityId}/>
                <div className="flex">
                    <ButtonAccept type="submit" className="w-6/12" text="Save"/>
                    <ButtonCancel className="w-6/12 ml-1" text="Delete" onClick={handleDelete}/>
                </div>
            </form>
          </div>
        </div>
      </>
    );
}