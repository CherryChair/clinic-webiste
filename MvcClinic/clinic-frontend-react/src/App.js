import './App.css';
import './routes'
import Cookies from 'js-cookie';
import './components/RouteGuard'
import MyRoutes from './routes';
import { isAdmin, isDoctor, isLoggedIn, isPatient, setAuthToken } from './pages/Login';
import Navbar from './components/Navbar';

const token = Cookies.get("token");
if (token) {
    setAuthToken(token);
}
const navigation = [
]

const patientNavigation = [
  { name: 'Schedules', href: '/schedules' },
]

const doctorNavigation = [
  { name: 'Patients', href: '/patients' },
  { name: 'Schedules', href: '/schedules' },
]

const adminNavigation = [
  { name: 'Employees', href: '/employees' },
  { name: 'Patients', href: '/patients' },
  { name: 'Schedules', href: '/schedules' },
]

function App() {
  let currentNavigation = navigation;
  if (isLoggedIn()) {
    if (isAdmin()) {
      currentNavigation = adminNavigation;
    } else if (isDoctor()) {
      currentNavigation = doctorNavigation;
    } else if (isPatient()) {
      currentNavigation = patientNavigation;
    }
  }
  
  return (
    <>
      <Navbar navigation={currentNavigation}/>
      <div className="App">
        <MyRoutes></MyRoutes>
      </div>
    </>
  );
}

export default App;
